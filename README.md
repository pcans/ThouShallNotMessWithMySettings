# ThouShallNotMessWithMySettings

## What this tool does

Watch Windows registry entries, and if something change them, change them back.

## Why I wrote it

I wrote this tool because some processes kept changing registry values on my windows computer, specifically my proxy.

I can't remove theses processes and need to live with them.

But bored of eternally set the same settings back, I wrote a tool that watch relevant keys in the registry, and if they change, change back to the value I want.

But it can be use to watch any entries you might want.

## How to use it
The tool is really easy to use, just start it and it will create an example config for you that you can re-use. I keep it's windows open because I like to see when it protect me from again, re-enter my proxy settings.

    ThouShallNotMessWithMySettings.exe [settings_file]

Parameters:

* **/h** - Display help
* **settings_file** - The filename to read keys from. default value if not specified is settings.xml, if not exists will be created.


## Example config file

    <?xml version="1.0" encoding="utf-8" ?>
    <keys>
        <key>
            <path>HKEY_CURRENT_USER\Software\Microsoft\Windows\CurrentVersion\Internet Settings</path>
            <name>ProxyEnable</name>
            <type>REG_DWORD</type>
            <value>1</value>
        </key>
        <key>
            <path>HKEY_CURRENT_USER\Software\Microsoft\Windows\CurrentVersion\Internet Settings</path>
            <name>ProxyServer</name>
            <type>REG_SZ</type>
            <value>myproxy.corporate.com:8080</value>
        </key>
        <key>
            <path>HKEY_CURRENT_USER\Software\Microsoft\Windows\CurrentVersion\Internet Settings\Connections</path>
            <name>SavedLegacySettings</name>
            <type>REG_BINARY</type>
            <value>46 00 00 00 00 00 00 00 03 00 00 00 2F 00 00 00</value>
        </key>
    </keys>


## Handled registry data type
Only 3 kinds of registry type can be used but the tool is open sauce, feel free to add more if you need:

*  REG_DWORD (uint32)
*  REG_SZ (string)
*  REG_BINARY (binary)

The binary data parser will ignore non hex characters, it will strip spaces, tabs, newline, etc… So feel free to indent your binary data! :)


## About the code
Since it is about Windows registry I made it using .net. But I only had *VS2005* installed when i made it so it is only *.net 2.0* and no *LINQ to XML* candy for me.

Licence is *Apache Software Licence*, enjoy.

I borrowed some the code that is watching the registry from this RegistryMonitor article, licensed under *CPOL*. [http://www.codeproject.com/KB/system/registrymonitor.aspx](http://www.codeproject.com/KB/system/registrymonitor.aspx)