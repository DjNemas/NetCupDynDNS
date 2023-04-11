# Netcup DynDNS application for domain records

This DynDNS application auto updates your IP destination in your records for your domain, based on the current IP this application is running.
The check interval is 1 Minute. Update only will be made, when the destination IP changed.
The Binary exist for Windows x64 and Linux x64 (not ARM achitecture).

### As Example:
Let us say u have a Server running on your home:
When this Application is running on this Server, it will auto update the domain record to the public IP of this server.

![Preview](https://djnemas.de/SX/WindowsTerminal_07hkX5nooe.gif)

# HowTo
## Both OS
On first startup a Credential.json file will be created on same directory of the binary file.
You have to edit and fill in your netcup credentials.
- Netcup Customer ID
- Netcup APIKey
- Netcup APIPassword
- Domain
Both API tokens can be generated in your CCP under "Stammdaten/>_API". (Keep them Secret!!!)

## Linux
Upload the LinuxX64 binary to your Server.
Give this file execute permissions with `chmod +x ./LinuxX64`
Then just start it with `./LinuxX64`

## Windows
You only have to Execute the WinX64.exe







