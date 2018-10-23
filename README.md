# Amendment of the license

DO NOT use this code, in part or in totality, to cheat, or produce code that would eventually lead to cheat.

# Overview

This application displays the current weapon usage, for Monster Hunter: World.

For those like me who always want to keep en eye on their weapon usage, consider it has an additional UI the game does not provide. Now you can have you weapon usage handy all the time.

![Screenshot](docs/screenshot01.png)

The application supports multiple users, and the 3 save slots per account.

The save data is automatically reloaded and the UI is seamlessly updated, so after each auto-save, the application will keep the display up to date.

# Interesting research

I've implemented an unsafe decryption method, but surprisingly it is as fast as the managed decryption method.

I was expecting it to be much fast since it does in-place decryption without tons of unnecessary copies. Also, parallelizing the unsafe method does not make it faster either.

Maybe the blowfish algorithm itself takes too much overhead for such optimizations to end up being insignificant.

# Thanks

v00d00y, Asterisk
