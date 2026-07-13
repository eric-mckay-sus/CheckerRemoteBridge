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

New software requirements

- Monitor state of 5 distinct devices running checker client program
- Auto-SSH login from interface to enable remote checksum
  - On successful login, run checksum script on checker client program, report result to user (need to know format of cksum output)
    - Compare checksum result to MES-provided value (either stored in OPC tag or ENV variable)
      - On successful checksum, auto-launch checker client
      - On unsuccessful checksum, show Pi online, but checker client offline. Provide option to attempt checksum/launch again
  - On unsuccessful login, show Pi as offline

- Continuously update green/red state based on status code and time of last update
  - Every update in checker client program paired with ping of current datetime to WatchdogDateTime OPC tag (>10 sec to red state)
  - Manual shutdown option always present (fire shutdown request, await ACK. Show Pi offline)
    - Checker client program responsible for gracefully closing and actual Pi shutdown
  - When checker in red state, offer reboot option. Fire RebootRequest and watch for code 100 (Show Pi offline after ACK)
    - Checker client program responsible for actual Pi reboot (Pi should launch checker client program on startup)

- Monitor status/alarm messages for visual state updates
- Periodic backup of checker logs to SUS-PE3DATA-02 drive (sFTP/SCP)
