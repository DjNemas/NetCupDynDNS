# Netcup DynDNS application for domain records

This DynDNS application auto updates your IP destination in your records for your domain, based on the current IP this application is running.
The check interval is 1 Minute. Update only will be made, when the destination IP changed.
The Binary exist for Windows x64 and Linux x64 (not ARM architecture).

### As Example:
Let us say you have a Server running on your home:
When this Application is running on this Server, it will auto update the domain record to the public IP of this server.

![Preview](https://djnemas.de/SX/WindowsTerminal_hczA9AShWq.gif)

# HowTo
## Both OS
On first startup a AccountInformation.json file will be created in same directory of the binary file.
You have to edit and fill in your netcup credentials.
- Netcup Customer ID
- Netcup APIKey
- Netcup APIPassword
- Domain

Both API tokens can be generated in your CCP under "Stammdaten/>_API" or respectively "Master Data/>_API". (Keep them Secret!!!)

## Alternatively use Environment variables

The program will also allow the usage of environment variables. 
You can set them in your environment variables according to your OS.
The following variables are supported

* NETCUP_API_KEY
* NETCUP_API_PASSWORD
* NETCUP_CUSTOMER_NUMBER
* NETCUP_DOMAIN
* NETCUP_DYNDNS_EXECUTION_INTERVAL_IN_MINUTES (default value: 60 minutes)
* NETCUP_IGNORED_HOSTS (default value: "*,@,autoconfig,db,google,key1._domainkey,key2._domainkey,mail,webmail")

### NETCUP_IGNORED_HOSTS

This environment variable can contain multiple values and is used like this

```bash
NETCUP_IGNORED_HOSTS="host1,host2,host3"
```

## Linux
Upload the LinuxX64 binary to your Server.
Give this file execute permissions with `chmod +x ./LinuxX64`
Then just start it with `./LinuxX64`

## Windows
You only have to Execute the WinX64.exe

## Docker

There is a docker image available in this repository or can be built from source in this repository.

### Build from source

Run
```bash
docker build -t netcup-dyndns .
```

### Running the container

You can run the container and pass the environment variables like this

```bash
docker run --name netcup-dyndns \
-e NETCUP_API_KEY="your_api_key" \
-e NETCUP_API_PASSWORD="your_api_password" \
-e NETCUP_CUSTOMER_NUMBER="your_customer_number" \
-e NETCUP_DOMAIN="example.com" \
-e NETCUP_DYNDNS_EXECUTION_INTERVAL_IN_MINUTES="your_interval" \
-e NETCUP_IGNORED_HOSTS="*,@,autoconfig,db,google,key1._domainkey,key2._domainkey,mail" \
netcup-dyndns
```





