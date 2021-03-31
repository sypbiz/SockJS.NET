@echo off

echo ====== Cleaning build folder ======
rm -f .build/*

echo ====== Building syp.biz.SockJS.NET.Client ======
dotnet build -c Release -o .build/ syp.biz\SockJS.NET\syp.biz.Stomp.Net.Client\

echo ====== Pushing generated packages ======
for %%v in (.build/*.nupkg) do nuget.exe push -Source "Synexie-packages" -ApiKey AzureDevOps .build/%%v
