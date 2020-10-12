const http = require("http");
const sockjs = require("sockjs");
const node_static = require('node-static');
let shutdown = false;

const echo = sockjs.createServer({ sockjs_url: 'http://cdn.jsdelivr.net/sockjs/1.0.1/sockjs.min.js' });
echo.on('connection', conn => {
    console.log("connection", conn.protocol, conn.url, conn.id);
    if (conn.headers) console.log(Object.entries(conn.headers).map(([h,v]) => `\t${h}:${v}`).join("\n"));
    conn.on('data', message => {
        console.log("data", ">>>", message);
        if (message === "shutdown") {
            message = "ok";
            console.log("data", "<<<", message);
            conn.write(message);
            shutdown = true;
        } else {
            console.log("data", "<<<", message);
            conn.write(message);
        }
    });
    conn.on('close', () => {
        console.log("close");
        if (shutdown) {
            console.log("exiting");
            process.exit(0);
        }
    });
});

const static_directory = new node_static.Server(__dirname);
const server = http.createServer();
server.addListener('request', (req, res) => {
    console.log("request", req.method, req.url);
    static_directory.serve(req, res);
});
server.addListener('upgrade', (req, res) => {
    console.log("upgrade");
    res.end();
});
echo.installHandlers(server, { prefix: '/echo' });
server.listen(9999, '0.0.0.0');