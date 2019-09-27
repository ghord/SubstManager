# SubstManager
Simple subst.exe automating tool inspired by git

# Usage

First you need to register a remote network share you want to subst
```
sm alias my-alias '\\my\network\path'
```

Only one alias at the time is active: you can switch between active aliases with `switch` command:
```
sm switch my-alias
```

You can check currently active alias with `sm status` command:
```
sm status
```

Get a list of all defined aliases with `sm alias` command:
```
sm alias
```

Remove existing alias with `sm unalias` command
```
sm unalias my-alias
```

To mount currently active alias (uses subst.exe) simply use `mount` command, but first you need to configure subst drive:
```
sm config subst.drive D:
sm mount
```
Switching to another alias with `sm switch` will automatically mount it for you.

Configuration settings are preserved across sessions in `sm.config` file in `%localappdata%\SubstManager`. You can view all config values with `sm config` command:
```
sm config
```

By default, all newly created aliases are in `remote` state. This means that drive is directly subst'ed to alias path. It is possible to locally cache files for performance reasons. You can switch to locally caching files with `sm local` command,
but first you need to configure local cache directory:
```
sm config cache.directory D:\cache
sm local
```
Switching to locally cached files causes all contents of alias path to be copied to local cache directory under `alias-name` subdirectory. This cache can be updated on demand with `sm update` command (using robocopy.exe):
```
sm update
```
You can also see the preview list of files that changed and need to be updated with `sm fetch` command.
```
sm fetch
```
To switch back to remote access, use `sm remote` command:
```
sm remote
```





