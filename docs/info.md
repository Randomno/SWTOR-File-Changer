As detailed in torformat.txt, each archive contains file tables with various fields describing where files are located, how big they are, and their hashed name. When the game launches, each archive's file tabled is verified against its "ft.sig" file. ft.sig is a 194 or 195 byte file with a 2 byte header and a signature. The signature has been signed with an RSA private key. When decrypted with the public key, it gives a 24 byte value where the middle 8 bytes are the hash. The game computes a hash of every entry in the archive's file tables in the following manner:

Take every file table entry from the archive
Remove the entry for ft.sig itself
Sort this list by primary hash
For each file table entry, add it to new buffer in this order:
Compressed length (4 bytes)
Is compressed? (2 bytes)
Primary hash (4 bytes)
Secondary hash (4 bytes)
Metadata checksum (4 bytes)
Metadata length (4 bytes)

Hash this buffer with hashlittle2.

This is then compared against the 8 byte value obtained from ft.sig. If they don't match, the archive is rejected.

Given this, it seems that file replacement is quite limited, as none of the fields used in the hash can be modified. However there appears to be another option: creating duplicate file entries. If two entries have the same primary/secondary hash, the signature check only checks the first entry, but the game uses the second entry. Therefore you can add file data to the end of the archive, and add an entry to point to that offset. I'm unaware yet if this has any limitations.

## Modifiying the client
Modifying swtor.exe to remove the signature check is another option. Pending an automated patching method:

Find the string "ft.sig" in the executable.
Go to the function that uses it.
Go to the caller of this function.
Patch the code following the call (e.g. change JNE to JMP)