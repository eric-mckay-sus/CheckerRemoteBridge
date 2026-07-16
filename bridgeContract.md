# Checker remote bridge contract

Date: 2026-07-16
Author: Eric McKay (co-op)

A lot of this is rehashed from the OPC spec sheet, but I wanted to add more detail specifically for the bridge-checker expectations.

- CheckerState
  - Checker client MUST set this tag to 0 upon termination
    - Failure to do so will cause auto-launch to skip on the next time the bridge is initialized.
    - This implies a graceful termination that I'm told does not currently exist
  - Checker client should set this tag to 100 immediately on launch to notify the user that the auto-launch succeeded

- WatchdogDateTime
  - Checker client MUST set this at least once every 10 seconds.
    - Failure to do so will assign that checker red health state (reboot option)
    - If this is unfeasible, the time to 'panic' can be adjusted.

- RebootRequest
  - Checker client MUST gracefully terminate (i.e. finish current operation, set CheckerState=0) and reboot the Pi
    - Checker client should set CheckerState to 102 upon receipt to notify the user (the sooner the better)
  - Checker client should pulse RebootRequestACK for 1 second within 1 second of this tag being set
    - This is good practice to verify the signal was recieved, but not strictly necessary as the bridge doesn't check it as of this date

- ShutdownRequest
  - Checker client MUST gracefully terminate (i.e. finish current operation, set CheckerState=0) and shut down the Pi
    - Checker client should set CheckerState to 101 upon receipt to notify the user (the sooner the better)
  - Checker client should pulse ShutdownRequestACK for 1 second within 1 second of this tag being set
    - This is good practice to verify the signal was recieved, but not strictly necessary as the bridge doesn't check it as of this date

- CheckerStatusMessage
  - Checker client should set this tag to reflect the details of the current operation, as this is the most detailed information available via the bridge

- CheckerAlarmMessage
  - Checker client MUST unset this after alarm is resolved, or checker will remain in red state (reboot option)
  - Checker client should set this tag whenever CheckerState = 1, as that will help the user diagnose the issue

As part of graceful termination, it would make sense to upload the log to SUS-PE3DATA, but that's not technically a bridge-checker related concern.  
There should be some TerminationRequest OPC tag created to allow the bridge to request a graceful termination without device shutdown
