﻿// Boot - in case this is the top module (will be replaced by another otherwise)
System.BootFS().writeMasterBoot("startshell createworkspace default initculture 'en' initframework launchone BindKraftIntroApp");

System.ShellShortcuts().regKeylaunchShortcut("Q", "launchapp BindKraftIntroApp");

//System.ShellShortcuts().regStartShortcut("BindKraftIntroApp", "launchone BindKraftIntroApp", "BindKraftIntroApp ...").appclass("BindKraftIntroApp");