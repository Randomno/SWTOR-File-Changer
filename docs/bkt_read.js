funcs.readFile[FILE_TYPE.BKT] = function readBKT(dv) {
  const magic = dv.getUint32(0, true); // PBUK = Prototype bucket?
  assert(magic === 0x4B554250, 'Expected magic PBUK but read ' + magic);

  const versionMajor = dv.getUint16(4, true);
  const versionMinor = dv.getUint16(6, true);
  assert(versionMajor === 2 && (versionMinor === 4 || versionMinor === 5), 'Expected version 2.4 or 2.5 but read ' + versionMajor + '.' + versionMinor);

  let pos = 8;
  const length = dv.byteLength - 12;

  let numNodes = 0;

  const nodes = [];
  while (pos < length) { //for each DBLB section
    const dblbLength = dv.getUint32(pos, true); pos += 4;
    const dblbStartOffset = pos;
    const dblbMagic = dv.getUint32(pos, true); pos += 4;
    assert(dblbMagic === 0x424c4244, 'Expected magic DBLB but read ' + dblbMagic);
    const dblbVersion = dv.getUint32(pos, true); pos += 4;
    assert(dblbVersion === 1 || dblbVersion === 2, 'Expected DBLB version to be 1 or 2 but was ' + dblbVersion);

    while (dblbStartOffset + dblbLength - pos >= 4) { //for each bucket node
      const startOffset = pos;
      const entryLength = dv.getUint32(pos, true); pos += 4;
      if (entryLength === 0) break;
      numNodes += 1;

      let id, dataOffset, bitset;
      if (dblbVersion === 1) {
        bitset = dv.getUint16(pos, true); pos += 2; // either 14 = ScriptRef or 15 = NodeRef
        dataOffset = dv.getUint16(pos, true); pos += 2;
        const idLo = dv.getUint32(pos, true); pos += 4;
        const idHi = dv.getUint32(pos, true); pos += 4;
        id = uint64(idLo, idHi); // always 0xE00000... = 1614...
      } else if (dblbVersion === 2) {
        pos += 4; // skip 4 zero bytes
        const idLo = dv.getUint32(pos, true); pos += 4;
        const idHi = dv.getUint32(pos, true); pos += 4;
        id = uint64(idLo, idHi); // always 0xE00000... = 1614...
        bitset = dv.getUint16(pos, true); pos += 2; // either 14 = ScriptRef or 15 = NodeRef
        dataOffset = dv.getUint16(pos, true); pos += 2;
      }

      const nameOffset = dv.getUint16(pos, true); pos += 2;
      pos += 2;//description offset - always empty string

      if (dblbVersion === 1) pos += 4;
      const baseClassLo = dv.getUint32(pos, true); pos += 4;
      const baseClassHi = dv.getUint32(pos, true); pos += 4;
      const baseClass = uint64(baseClassLo, baseClassHi);
      if (dblbVersion === 2) pos += 4;
      pos += 2; // skip number of glommed classes
      pos += 2; // skip offset to glommed classes
      const uncomprLength = dv.getUint16(pos, true); pos += 2;//bugged, this is not actually the uncompressed length
      pos += 2; // skip zero bytes
      const uncomprOffset = dv.getUint16(pos, true); pos += 2;
      pos += 2;
      const streamStyle = dv.getUint8(pos++);

      const name = readString(dv, startOffset + nameOffset);
      const dataLength = entryLength - dataOffset;

      let compression = 'None';
      if (bitset === 15) {
        const comprMagic = dv.getUint16(startOffset + dataOffset, 16);
        if (comprMagic === 0x9c78) {
          compression = 'DEFLATE';
        } else if (comprMagic === 0xb528) {
          compression = 'zstd';
        } else {
          compression = 'Unknown (0x' + comprMagic.toString(16) + ')';
        }
      }

      nodes.push({ id, name, dataLength, compression });

      pos = dblbStartOffset + ((startOffset - dblbStartOffset + entryLength + 7) & 0xFFFFFF8); // Skip padding to next 8-byte block
    }

    pos = dblbStartOffset + dblbLength;
  }
  assert(nodes.length === numNodes, 'Expected to read ' + numNodes + ' nodes but read ' + nodes.length + ' nodes instead.');

  return { numNodes, nodes };
};
