#!/bin/bash
# Example start script for DynDNS application
# Copy this file to start.sh and fill in your credentials
# Make it executable: chmod +x start.sh

# ====================================================================
# REQUIRED PARAMETERS
# ====================================================================

# Your Netcup API Key (generate in CCP under "Stammdaten/API")
API_KEY="your_api_key_here"

# Your Netcup API Password
API_PASSWORD="your_api_password_here"

# Your Netcup Customer Number
CUSTOMER_NUMBER=123456

# Your domain name
DOMAIN="example.com"


# ====================================================================
# OPTIONAL PARAMETERS
# ====================================================================

# Execution interval in minutes (default: 60)
EXECUTION_INTERVAL=60

# Hostnames to ignore (space-separated, wrap each with -i)
# Leave empty to update ALL hosts (default behavior)
# Example: IGNORED_HOSTS="-i * -i @ -i autoconfig -i mail -i webmail"
IGNORED_HOSTS=""

# Enable ticking clock (true/false)
TICKING_CLOCK=false

# Save configuration to file (true/false)
# WARNING: Stores credentials in plain text!
SAVE_CONFIG=false


# ====================================================================
# START APPLICATION
# ====================================================================

echo "Starting DynDNS application..."
echo ""

# Build command
CMD="./DynDNS --apiKey \"$API_KEY\" --api-password \"$API_PASSWORD\" --customer-number $CUSTOMER_NUMBER --domain \"$DOMAIN\" --execution-interval-in-minutes $EXECUTION_INTERVAL"

# Add ignored hosts if specified
if [ -n "$IGNORED_HOSTS" ]; then
    CMD="$CMD $IGNORED_HOSTS"
fi

# Add optional flags
if [ "$TICKING_CLOCK" = "true" ]; then
    CMD="$CMD --ticking-clock"
fi

if [ "$SAVE_CONFIG" = "true" ]; then
    CMD="$CMD --save-config"
fi

# Execute command
eval "$CMD"

read -p "Press Enter to continue..."
