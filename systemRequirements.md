# Checker Remote Bridge

These will be incrementally improved, but this is currently just a space to collect context.

Current state:

- Pi5s running Raspbian OS Full
- Currently, power on requires physical login/SSH (as 'eng' user rather than sudo)
- From there, user must manually launch shell script
- No application state monitoring
- Pis are connected to a battery backup (UPS), auto-boot on power loss
- Pis run on SD cards susceptible to corruption/longevity issues
- Checksum can be calculated manually using particular shell script

Dev requirements

- Monitor state of 5 different checkers
- Auto-launch/login from interface
- Display checksum on individual checker startup, report to OPC
- Compare actual to expected interface-side (fetched from OPC, provided by MES)
- Periodic backup of checker logs to SUS-PE3DATA-02 drive (sFTP/SCP)
- Manual shutdown option always present (fire shutdown request)
- When checker in red state, offer reboot option. Fire RebootRequest and watch for code 100
- Monitor status/alarm messages for visual state updates
- Continuously update green/red state based on latest WatchdogDateTime (~10 sec max before triggering red)
