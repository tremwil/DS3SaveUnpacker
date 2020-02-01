# DS3SavePatcher

Simple console program with a few features related to Dark Souls III saves:
- Pack/Unpack encrypted Dark Souls III save (SL2) files
- Pack/Unpack unencrypted BND4 archives (Supports only those with 32 byte headers)
- Patch a Dark Souls III save (SL2) file to change the linked account, allowing save transfer across accounts.

This program was specifically made for this last feature, and serves as an alternative to the DS3 Save Manager, http://l3g.space/files/SystemTest/?Main_Page:Dark_Souls_III. The DS3 Save Manager plays with the game's process and **may** thus cause a soft ban (*Note: I don't know if the author avoided this or how good the game's code integrity check is*). This tool directly modifies the linked Steam account ID stored in the save file, and **should** pass the save consistency check, *IF the provided safe is already valid*. However, it is very new and some testing will be required to ensure this.

## Regarding unpacking/packing saves
When unpacking a save, you get 12 `USER_DATA XXX` files. Those numbered `000` to `009` are the character save slots. I know `010` stores some global information (including the linked account ID), but I have not looked deeply into it. Similarly, the `011` entry appears to be in the DCX format, and I did not investigate its contents. 

**Note that loading a modified save back into the game can lead to a softban. Either experiment on an account that is already penalized or completely block the game's connection to the Internet (_being offline on Steam is NOT enough_), and be sure you know what you are doing!**
