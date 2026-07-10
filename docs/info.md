As detailed in torformat.txt, each .tor archive contains file tables with various fields describing where files are located in the archive, their size, and their hashed name. When the client launches, each archive's file tables are verified against its "ft.sig" file, which has been signed with an RSA key. Each file entry's stored length, hash, metadata length, metadata checksum, and compression type fields are used in the check, and the client will reject the archive if the check fails.

Given this, it would seem that file replacement is limited, as only the offset and uncompressed length of an entry can be modified. However there is a way around this: creating duplicate file entries. If two entries have the same hash, the signature check only uses the first entry, but the client uses the second entry. Therefore you can add file data to the end of the archive, and add an entry to point to that offset.

The file changer uses different methods depending on the replacement.

If a replacement's stored size is the same as the original, it will be replaced at its original offset.

If an entry is compressed and the replacement is at least 8 bytes smaller than the original, it will be replaced at its original offset with a [skippable frame](https://github.com/facebook/zstd/blob/dev/doc/zstd_compression_format.md#skippable-frames) added at the end. In both cases the uncompressed length field of the entry will also be updated.

If a replacement is larger than the original, a duplicate entry will be added as described above. The metadata is disregarded for simplicity and the default checksum of 0xDEADBEEF is written in the entry.

If a replacement is between 1 and 7 bytes smaller than the original, a duplicate entry will be added. It would be possible to try compressing at different levels (a feature of zstd) to try to match the original size exactly, but this is not yet implemented. It may also be possible to add data at the end of the replacement file until the compressed size matches, and update the entry's uncompressed length to be the replacement's size before this data was added.

If a file is a dependency (see below), a duplicate entry will be added. The proper way to determine dependencies is to read /resources/global.dep, but currently it is just determined by file extension for simplicity.

If a file is uncompressed and smaller than the original, a duplicate entry will be added. For some file types such as .gfx, replacing in place would be safe as trailing 0 bytes would be disregarded, but it's uncertain what types are safe or unsafe. It's also unknown if changing the uncompressed length field would help.

## Dependent files
Some file types are dependencies of other files (.mph -> .jba, .gr2 -> .mat, .tex, .tiny.dds). When loading a file with dependencies, the client appears to only check subsequent entries to find its dependencies. Therefore a dependency cannot be located earlier in the archive than its owner, or it will fail to load.

## Node replacement
Node replacement is simpler as there are no integrity checks; the entry is simply replaced with a new entry. The file changer currently assumes the node's description will be empty, which is the case for almost all nodes throughout the game's history.

Only nodes in .bkt files are currently supported, but prototype nodes (.node) can be replaced using file replacement.

## Modifying the client
Modifying swtor.exe to remove the signature check is another option. One way to do it:

Find the string "ft.sig" in the executable.
Go to the function that uses it.
Go to the caller of this function.
Patch the code following the call (e.g. change JNE to JMP)