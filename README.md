# Netcup DynDNS application for domain records

**Version 3.0.0**

This DynDNS application auto updates your IP destination in your records for your domain, based on the current IP this application is running.
The check interval is configurable (default: 60 minutes). Update only will be made when the destination IP changed.
The Binary exists for Windows x64 and Linux x64 (not ARM architecture).

### As Example:
Let us say you have a Server running on your home:
When this Application is running on this Server, it will auto update the domain record to the public IP of this server.

![Preview v3.0.0](https://github.com/user-attachments/assets/f9aeeb58-ff93-4baa-9704-26342dc094fe)

# Technical Details

This application is built with .NET 10 and uses a custom lightweight CLI framework (`DynDNS.Cli`) for command-line argument parsing and execution, with no external dependencies.

# HowTo
## Configuration Priority

The application supports three ways to provide configuration with the following priority:

**Priority 1: Command-line parameters** (highest)  
**Priority 2: Configuration file** (`dyndns-updater-config.json`)  
**Priority 3: Environment variables** (lowest)

This means CLI parameters override config file values, which in turn override environment variables.

## Both OS

You can run the application via `./DynDNS` (Linux) or `DynDNS.exe` (Windows).

If you run the application without any parameters or with invalid parameters, it will automatically display help information that explains the different command line options.

### Parameter Groups

#### 1. Credentials (Required via at least one method)

These parameters are **required** but can be provided via CLI, config file, or environment variables:

* `--apiKey` (or `-k`) - Your Netcup API Key
* `--api-password` (or `-p`) - Your Netcup API Password
* `--customer-number` (or `-n`) - Your Netcup Customer Number
* `--domain` (or `-d`) - Your domain name

Both API tokens can be generated in your CCP under "Stammdaten/>_API" or respectively "Master Data/>_API". (Keep them Secret!!!)

```bash
./DynDNS --apiKey "your_key" --api-password "your_password" --customer-number 123456 --domain "example.com"
```

#### 2. Configuration File Management

* `--save-config` (or `-s`) - Stores your parameters into a configuration file (`dyndns-updater-config.json`) which will be used in future runs with Priority 2.

**⚠ Important for upgrades from older versions:**

In previous versions, the application **only** worked with a configuration file. Starting with this version, you can now use CLI parameters, config file, or environment variables with the priority system described above.

- If you set `--save-config`, your current parameters will be saved to the config file
- If you **don't** set `--save-config`, the application will look for an existing config file and **delete it** to avoid potentially leaking credentials
- The config file is stored in plain text, so use with caution!

**Example for migration:**
```bash
# First run: Save your config
./DynDNS --apiKey "key" --api-password "pass" --customer-number 123456 --domain "example.com" --save-config

# Edit the generated dyndns-updater-config.json if needed

# Subsequent runs: Config file will be used automatically
./DynDNS
```

#### 3. Ignored Hostnames (Optional)

* `--ignored-hostname` (or `-i`) - You can list the hostnames that will be ignored from the update process. Can be specified multiple times.

**Example:**
```bash
./DynDNS -i * -i @ -i autoconfig -i mail -i webmail
```

**Note:** If no ignored hostnames are specified via CLI, config file, or environment variable, **all hosts will be updated**. There are no default ignored hosts.

#### 4. Execution Control (Optional)

* `--execution-interval-in-minutes` (or `-e`) - Interval in minutes for checking if IP has changed (default: 60 minutes)
  - Set to `0` to run once and exit
  - Set to any positive number to keep the application running and check periodically

* `--ticking-clock` (or `-t`) - Display a live countdown in the terminal until the next execution
  - Without this flag: Shows a single message with the next execution time
  - With this flag: Shows a ticking countdown timer

**Examples:**
```bash
# Run once and exit
./DynDNS --execution-interval-in-minutes 0

# Check every 30 minutes with countdown
./DynDNS --execution-interval-in-minutes 30 --ticking-clock

# Check every hour with static message (default behavior)
./DynDNS --execution-interval-in-minutes 60
```

## Using Configuration Files

The application can save your configuration to a JSON file (`dyndns-updater-config.json`) for easier reuse.

### Configuration File Structure:

When you use `--save-config`, a JSON file will be created with the following structure:

```json
{
  "Credentials": {
    "Domain": "example.com",
    "ApiCustomerNumber": 123456,
    "ApiClientPW": "your_api_password_here",
    "ApiClientKey": "your_api_key_here"
  },
  "IgnoredHosts": {
    "Hostnames": [
      "*",
      "@",
      "autoconfig",
      "mail",
      "webmail"
    ]
  }
}
```

**Notes:**
- Set `Hostnames` to an empty array `[]` to update all hosts
- When creating a config file for the first time without specifying `-i` parameters, example hosts will be automatically added
- The file is stored in the same directory as the executable
- **Warning:** This file contains credentials in plain text!

### Example start scripts

For convenience, you can copy and modify the example start scripts:

**Windows:** Copy `start.bat.example` to `start.bat` and fill in your credentials  
**Linux:** Copy `start.sh.example` to `start.sh`, fill in your credentials, and make it executable with `chmod +x start.sh`

These scripts provide a template for all available parameters.

## Using Environment Variables

The program also supports environment variables as the lowest priority option.

The following variables are supported:

* `NETCUP_API_KEY`
* `NETCUP_API_PASSWORD`
* `NETCUP_CUSTOMER_NUMBER`
* `NETCUP_DOMAIN`
* `NETCUP_DYNDNS_EXECUTION_INTERVAL_IN_MINUTES` (default value: 60 minutes)
* `NETCUP_IGNORED_HOSTS` (optional, comma-separated list of hostnames to ignore)

### NETCUP_IGNORED_HOSTS

This environment variable can contain multiple values (comma-separated):

```bash
NETCUP_IGNORED_HOSTS="host1,host2,host3"
```

**Note:** If this variable is not set, all hosts will be updated. There are no default ignored hosts.

## Platform-Specific Instructions

### Linux
1. Upload the LinuxX64 binary to your Server
2. Give this file execute permissions with `chmod +x ./DynDNS`
3. Start it with `./DynDNS` (with parameters, config file, or environment variables)

### Windows
Simply execute `DynDNS.exe` (with parameters, config file, or environment variables)

## Docker

There is a docker image available in this repository or can be built from source.

### Build from source

```bash
docker build -t netcup-dyndns .
```

### Running the container

You can run the container and pass the environment variables:

```bash
docker run --name netcup-dyndns \
-e NETCUP_API_KEY="your_api_key" \
-e NETCUP_API_PASSWORD="your_api_password" \
-e NETCUP_CUSTOMER_NUMBER="your_customer_number" \
-e NETCUP_DOMAIN="example.com" \
-e NETCUP_DYNDNS_EXECUTION_INTERVAL_IN_MINUTES="60" \
ghcr.io/djnemas/netcupdyndns:latest
```

**Note:** Add `-e NETCUP_IGNORED_HOSTS="host1,host2,host3"` if you want to ignore specific hosts.

### Using Docker Compose

Create a `docker-compose.yml` file:

```yaml
services:
  netcup-dyndns:
    image: ghcr.io/djnemas/netcupdyndns:latest
    container_name: netcup-dyndns
    environment:
      - NETCUP_API_KEY=your_api_key
      - NETCUP_API_PASSWORD=your_api_password
      - NETCUP_CUSTOMER_NUMBER=your_customer_number
      - NETCUP_DOMAIN=example.com
      - NETCUP_DYNDNS_EXECUTION_INTERVAL_IN_MINUTES=60
      # - NETCUP_IGNORED_HOSTS=host1,host2,host3  # Optional: Add if you want to ignore specific hosts
    restart: unless-stopped
```

Then run:
```bash
docker-compose up -d
```

## Complete Usage Examples

### Example 1: Quick start with CLI parameters
```bash
./DynDNS --apiKey "abc123" --api-password "xyz789" --customer-number 123456 --domain "example.com"
```

### Example 2: Save config for future use
```bash
./DynDNS -k "abc123" -p "xyz789" -n 123456 -d "example.com" -s
```

### Example 3: Use saved config with custom ignored hosts
```bash
./DynDNS -i mail -i webmail -i ftp
```

### Example 4: One-time update using environment variables
```bash
export NETCUP_API_KEY="abc123"
export NETCUP_API_PASSWORD="xyz789"
export NETCUP_CUSTOMER_NUMBER="123456"
export NETCUP_DOMAIN="example.com"
./DynDNS --execution-interval-in-minutes 0
```

### Example 5: Keep alive with ticking countdown
```bash
./DynDNS -k "abc123" -p "xyz789" -n 123456 -d "example.com" -e 30 -t
