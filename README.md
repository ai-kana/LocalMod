# LocalMod
 Client side modding framework for Unturned
 Extremely WIP

## Planned Features
Features to be added in future
 - Put this on nuget
 - Hot reloading
 - Abstracted RPCs
 - Abstracted events
 - General unity abstractions
 - Automatic plugin reporting and installing when joining a server

## Contribution and styling
Basic styling for contributing if you feel inclined
 - New Line braces
 - Private members begin with an underscore
 - All members first letter is uppercase
 - Local variables being with a lowercase
 - One type per-file (Excluding delegates)
 - File name is the same as its type

## Plugins
Plugins are defined in by the interface `LocalMod.Plugins.IPlugin`
To load your plugin put it in `Unturned/LocalMod/Plugins`

## Configuration
To load and save configuration use `LocalMod.Configuration.ConfigSaver` and supply a name
Your config file will be created in JSON in `Unturned/LocalMod/Config/<name>.json`

## Commands
A questionable system that maybe removed for queueing and executing this in bulk
Currently exists
 - `LocalMod.Commands.AsyncCommandWorker` runs actions on the threadpool
 - `LocalMod.Commands.RoutineCommandWorker` runs action in a coroutine once every frame
