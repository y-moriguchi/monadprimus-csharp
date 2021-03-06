/*
 * This source code is under the Unlicense
 */
using System;
using System.Text;

namespace Morilib
{
    public static partial class MonadPrimus
    {
        private const string JavaIdentifierRegex =
            "[\x0024\x0041-\x005a\x005f\x0061-\x007a\x00a2-\x00a5\x00aa\x00b5\x00ba\x00c0-\x00d6\x00d8-\x00f6\x00f8-\x02c1\x02c6-\x02d1\x02e0-\x02e4\x02ec\x02ee" +
            "\x0370-\x0374\x0376-\x0377\x037a-\x037d\x0386\x0388-\x038a\x038c\x038e-\x03a1\x03a3-\x03f5\x03f7-\x0481\x048a-\x0527\x0531-\x0556\x0559\x0561-\x0587" +
            "\x05d0-\x05ea\x05f0-\x05f2\x060b\x0620-\x064a\x066e-\x066f\x0671-\x06d3\x06d5\x06e5-\x06e6\x06ee-\x06ef\x06fa-\x06fc\x06ff\x0710\x0712-\x072f" +
            "\x074d-\x07a5\x07b1\x07ca-\x07ea\x07f4-\x07f5\x07fa\x0800-\x0815\x081a\x0824\x0828\x0840-\x0858\x0904-\x0939\x093d\x0950\x0958-\x0961\x0971-\x0977" +
            "\x0979-\x097f\x0985-\x098c\x098f-\x0990\x0993-\x09a8\x09aa-\x09b0\x09b2\x09b6-\x09b9\x09bd\x09ce\x09dc-\x09dd\x09df-\x09e1\x09f0-\x09f3\x09fb" +
            "\x0a05-\x0a0a\x0a0f-\x0a10\x0a13-\x0a28\x0a2a-\x0a30\x0a32-\x0a33\x0a35-\x0a36\x0a38-\x0a39\x0a59-\x0a5c\x0a5e\x0a72-\x0a74\x0a85-\x0a8d\x0a8f-\x0a91" +
            "\x0a93-\x0aa8\x0aaa-\x0ab0\x0ab2-\x0ab3\x0ab5-\x0ab9\x0abd\x0ad0\x0ae0-\x0ae1\x0af1\x0b05-\x0b0c\x0b0f-\x0b10\x0b13-\x0b28\x0b2a-\x0b30\x0b32-\x0b33" +
            "\x0b35-\x0b39\x0b3d\x0b5c-\x0b5d\x0b5f-\x0b61\x0b71\x0b83\x0b85-\x0b8a\x0b8e-\x0b90\x0b92-\x0b95\x0b99-\x0b9a\x0b9c\x0b9e-\x0b9f\x0ba3-\x0ba4" +
            "\x0ba8-\x0baa\x0bae-\x0bb9\x0bd0\x0bf9\x0c05-\x0c0c\x0c0e-\x0c10\x0c12-\x0c28\x0c2a-\x0c33\x0c35-\x0c39\x0c3d\x0c58-\x0c59\x0c60-\x0c61\x0c85-\x0c8c" +
            "\x0c8e-\x0c90\x0c92-\x0ca8\x0caa-\x0cb3\x0cb5-\x0cb9\x0cbd\x0cde\x0ce0-\x0ce1\x0cf1-\x0cf2\x0d05-\x0d0c\x0d0e-\x0d10\x0d12-\x0d3a\x0d3d\x0d4e" +
            "\x0d60-\x0d61\x0d7a-\x0d7f\x0d85-\x0d96\x0d9a-\x0db1\x0db3-\x0dbb\x0dbd\x0dc0-\x0dc6\x0e01-\x0e30\x0e32-\x0e33\x0e3f-\x0e46\x0e81-\x0e82\x0e84" +
            "\x0e87-\x0e88\x0e8a\x0e8d\x0e94-\x0e97\x0e99-\x0e9f\x0ea1-\x0ea3\x0ea5\x0ea7\x0eaa-\x0eab\x0ead-\x0eb0\x0eb2-\x0eb3\x0ebd\x0ec0-\x0ec4\x0ec6" +
            "\x0edc-\x0edd\x0f00\x0f40-\x0f47\x0f49-\x0f6c\x0f88-\x0f8c\x1000-\x102a\x103f\x1050-\x1055\x105a-\x105d\x1061\x1065-\x1066\x106e-\x1070\x1075-\x1081" +
            "\x108e\x10a0-\x10c5\x10d0-\x10fa\x10fc\x1100-\x1248\x124a-\x124d\x1250-\x1256\x1258\x125a-\x125d\x1260-\x1288\x128a-\x128d\x1290-\x12b0\x12b2-\x12b5" +
            "\x12b8-\x12be\x12c0\x12c2-\x12c5\x12c8-\x12d6\x12d8-\x1310\x1312-\x1315\x1318-\x135a\x1380-\x138f\x13a0-\x13f4\x1401-\x166c\x166f-\x167f\x1681-\x169a" +
            "\x16a0-\x16ea\x16ee-\x16f0\x1700-\x170c\x170e-\x1711\x1720-\x1731\x1740-\x1751\x1760-\x176c\x176e-\x1770\x1780-\x17b3\x17d7\x17db-\x17dc\x1820-\x1877" +
            "\x1880-\x18a8\x18aa\x18b0-\x18f5\x1900-\x191c\x1950-\x196d\x1970-\x1974\x1980-\x19ab\x19c1-\x19c7\x1a00-\x1a16\x1a20-\x1a54\x1aa7\x1b05-\x1b33" +
            "\x1b45-\x1b4b\x1b83-\x1ba0\x1bae-\x1baf\x1bc0-\x1be5\x1c00-\x1c23\x1c4d-\x1c4f\x1c5a-\x1c7d\x1ce9-\x1cec\x1cee-\x1cf1\x1d00-\x1dbf\x1e00-\x1f15" +
            "\x1f18-\x1f1d\x1f20-\x1f45\x1f48-\x1f4d\x1f50-\x1f57\x1f59\x1f5b\x1f5d\x1f5f-\x1f7d\x1f80-\x1fb4\x1fb6-\x1fbc\x1fbe\x1fc2-\x1fc4\x1fc6-\x1fcc" +
            "\x1fd0-\x1fd3\x1fd6-\x1fdb\x1fe0-\x1fec\x1ff2-\x1ff4\x1ff6-\x1ffc\x203f-\x2040\x2054\x2071\x207f\x2090-\x209c\x20a0-\x20b9\x2102\x2107\x210a-\x2113" +
            "\x2115\x2119-\x211d\x2124\x2126\x2128\x212a-\x212d\x212f-\x2139\x213c-\x213f\x2145-\x2149\x214e\x2160-\x2188\x2c00-\x2c2e\x2c30-\x2c5e\x2c60-\x2ce4" +
            "\x2ceb-\x2cee\x2d00-\x2d25\x2d30-\x2d65\x2d6f\x2d80-\x2d96\x2da0-\x2da6\x2da8-\x2dae\x2db0-\x2db6\x2db8-\x2dbe\x2dc0-\x2dc6\x2dc8-\x2dce\x2dd0-\x2dd6" +
            "\x2dd8-\x2dde\x2e2f\x3005-\x3007\x3021-\x3029\x3031-\x3035\x3038-\x303c\x3041-\x3096\x309d-\x309f\x30a1-\x30fa\x30fc-\x30ff\x3105-\x312d\x3131-\x318e" +
            "\x31a0-\x31ba\x31f0-\x31ff\x3400-\x4db5\x4e00-\x9fcb\xa000-\xa48c\xa4d0-\xa4fd\xa500-\xa60c\xa610-\xa61f\xa62a-\xa62b\xa640-\xa66e\xa67f-\xa697" +
            "\xa6a0-\xa6ef\xa717-\xa71f\xa722-\xa788\xa78b-\xa78e\xa790-\xa791\xa7a0-\xa7a9\xa7fa-\xa801\xa803-\xa805\xa807-\xa80a\xa80c-\xa822\xa838\xa840-\xa873" +
            "\xa882-\xa8b3\xa8f2-\xa8f7\xa8fb\xa90a-\xa925\xa930-\xa946\xa960-\xa97c\xa984-\xa9b2\xa9cf\xaa00-\xaa28\xaa40-\xaa42\xaa44-\xaa4b\xaa60-\xaa76\xaa7a" +
            "\xaa80-\xaaaf\xaab1\xaab5-\xaab6\xaab9-\xaabd\xaac0\xaac2\xaadb-\xaadd\xab01-\xab06\xab09-\xab0e\xab11-\xab16\xab20-\xab26\xab28-\xab2e\xabc0-\xabe2" +
            "\xac00-\xd7a3\xd7b0-\xd7c6\xd7cb-\xd7fb\xf900-\xfa2d\xfa30-\xfa6d\xfa70-\xfad9\xfb00-\xfb06\xfb13-\xfb17\xfb1d\xfb1f-\xfb28\xfb2a-\xfb36\xfb38-\xfb3c" +
            "\xfb3e\xfb40-\xfb41\xfb43-\xfb44\xfb46-\xfbb1\xfbd3-\xfd3d\xfd50-\xfd8f\xfd92-\xfdc7\xfdf0-\xfdfc\xfe33-\xfe34\xfe4d-\xfe4f\xfe69\xfe70-\xfe74" +
            "\xfe76-\xfefc\xff04\xff21-\xff3a\xff3f\xff41-\xff5a\xff66-\xffbe\xffc2-\xffc7\xffca-\xffcf\xffd2-\xffd7\xffda-\xffdc\xffe0-\xffe1\xffe5-\xffe6]" +
            "[\x0024\x0030-\x0039\x0041-\x005a\x005f\x0061-\x007a\x007f-\x009f\x00a2-\x00a5\x00aa\x00ad\x00b5\x00ba\x00c0-\x00d6" +
            "\x00d8-\x00f6\x00f8-\x02c1\x02c6-\x02d1\x02e0-\x02e4\x02ec\x02ee\x0300-\x0374\x0376-\x0377\x037a-\x037d\x0386\x0388-\x038a\x038c\x038e-\x03a1" +
            "\x03a3-\x03f5\x03f7-\x0481\x0483-\x0487\x048a-\x0527\x0531-\x0556\x0559\x0561-\x0587\x0591-\x05bd\x05bf\x05c1-\x05c2\x05c4-\x05c5\x05c7\x05d0-\x05ea" +
            "\x05f0-\x05f2\x0600-\x0603\x060b\x0610-\x061a\x0620-\x0669\x066e-\x06d3\x06d5-\x06dd\x06df-\x06e8\x06ea-\x06fc\x06ff\x070f-\x074a\x074d-\x07b1" +
            "\x07c0-\x07f5\x07fa\x0800-\x082d\x0840-\x085b\x0900-\x0963\x0966-\x096f\x0971-\x0977\x0979-\x097f\x0981-\x0983\x0985-\x098c\x098f-\x0990\x0993-\x09a8" +
            "\x09aa-\x09b0\x09b2\x09b6-\x09b9\x09bc-\x09c4\x09c7-\x09c8\x09cb-\x09ce\x09d7\x09dc-\x09dd\x09df-\x09e3\x09e6-\x09f3\x09fb\x0a01-\x0a03\x0a05-\x0a0a" +
            "\x0a0f-\x0a10\x0a13-\x0a28\x0a2a-\x0a30\x0a32-\x0a33\x0a35-\x0a36\x0a38-\x0a39\x0a3c\x0a3e-\x0a42\x0a47-\x0a48\x0a4b-\x0a4d\x0a51\x0a59-\x0a5c\x0a5e" +
            "\x0a66-\x0a75\x0a81-\x0a83\x0a85-\x0a8d\x0a8f-\x0a91\x0a93-\x0aa8\x0aaa-\x0ab0\x0ab2-\x0ab3\x0ab5-\x0ab9\x0abc-\x0ac5\x0ac7-\x0ac9\x0acb-\x0acd\x0ad0" +
            "\x0ae0-\x0ae3\x0ae6-\x0aef\x0af1\x0b01-\x0b03\x0b05-\x0b0c\x0b0f-\x0b10\x0b13-\x0b28\x0b2a-\x0b30\x0b32-\x0b33\x0b35-\x0b39\x0b3c-\x0b44\x0b47-\x0b48" +
            "\x0b4b-\x0b4d\x0b56-\x0b57\x0b5c-\x0b5d\x0b5f-\x0b63\x0b66-\x0b6f\x0b71\x0b82-\x0b83\x0b85-\x0b8a\x0b8e-\x0b90\x0b92-\x0b95\x0b99-\x0b9a\x0b9c" +
            "\x0b9e-\x0b9f\x0ba3-\x0ba4\x0ba8-\x0baa\x0bae-\x0bb9\x0bbe-\x0bc2\x0bc6-\x0bc8\x0bca-\x0bcd\x0bd0\x0bd7\x0be6-\x0bef\x0bf9\x0c01-\x0c03\x0c05-\x0c0c" +
            "\x0c0e-\x0c10\x0c12-\x0c28\x0c2a-\x0c33\x0c35-\x0c39\x0c3d-\x0c44\x0c46-\x0c48\x0c4a-\x0c4d\x0c55-\x0c56\x0c58-\x0c59\x0c60-\x0c63\x0c66-\x0c6f" +
            "\x0c82-\x0c83\x0c85-\x0c8c\x0c8e-\x0c90\x0c92-\x0ca8\x0caa-\x0cb3\x0cb5-\x0cb9\x0cbc-\x0cc4\x0cc6-\x0cc8\x0cca-\x0ccd\x0cd5-\x0cd6\x0cde\x0ce0-\x0ce3" +
            "\x0ce6-\x0cef\x0cf1-\x0cf2\x0d02-\x0d03\x0d05-\x0d0c\x0d0e-\x0d10\x0d12-\x0d3a\x0d3d-\x0d44\x0d46-\x0d48\x0d4a-\x0d4e\x0d57\x0d60-\x0d63\x0d66-\x0d6f" +
            "\x0d7a-\x0d7f\x0d82-\x0d83\x0d85-\x0d96\x0d9a-\x0db1\x0db3-\x0dbb\x0dbd\x0dc0-\x0dc6\x0dca\x0dcf-\x0dd4\x0dd6\x0dd8-\x0ddf\x0df2-\x0df3\x0e01-\x0e3a" +
            "\x0e3f-\x0e4e\x0e50-\x0e59\x0e81-\x0e82\x0e84\x0e87-\x0e88\x0e8a\x0e8d\x0e94-\x0e97\x0e99-\x0e9f\x0ea1-\x0ea3\x0ea5\x0ea7\x0eaa-\x0eab\x0ead-\x0eb9" +
            "\x0ebb-\x0ebd\x0ec0-\x0ec4\x0ec6\x0ec8-\x0ecd\x0ed0-\x0ed9\x0edc-\x0edd\x0f00\x0f18-\x0f19\x0f20-\x0f29\x0f35\x0f37\x0f39\x0f3e-\x0f47\x0f49-\x0f6c" +
            "\x0f71-\x0f84\x0f86-\x0f97\x0f99-\x0fbc\x0fc6\x1000-\x1049\x1050-\x109d\x10a0-\x10c5\x10d0-\x10fa\x10fc\x1100-\x1248\x124a-\x124d\x1250-\x1256\x1258" +
            "\x125a-\x125d\x1260-\x1288\x128a-\x128d\x1290-\x12b0\x12b2-\x12b5\x12b8-\x12be\x12c0\x12c2-\x12c5\x12c8-\x12d6\x12d8-\x1310\x1312-\x1315\x1318-\x135a" +
            "\x135d-\x135f\x1380-\x138f\x13a0-\x13f4\x1401-\x166c\x166f-\x167f\x1681-\x169a\x16a0-\x16ea\x16ee-\x16f0\x1700-\x170c\x170e-\x1714\x1720-\x1734" +
            "\x1740-\x1753\x1760-\x176c\x176e-\x1770\x1772-\x1773\x1780-\x17d3\x17d7\x17db-\x17dd\x17e0-\x17e9\x180b-\x180d\x1810-\x1819\x1820-\x1877\x1880-\x18aa" +
            "\x18b0-\x18f5\x1900-\x191c\x1920-\x192b\x1930-\x193b\x1946-\x196d\x1970-\x1974\x1980-\x19ab\x19b0-\x19c9\x19d0-\x19d9\x1a00-\x1a1b\x1a20-\x1a5e" +
            "\x1a60-\x1a7c\x1a7f-\x1a89\x1a90-\x1a99\x1aa7\x1b00-\x1b4b\x1b50-\x1b59\x1b6b-\x1b73\x1b80-\x1baa\x1bae-\x1bb9\x1bc0-\x1bf3\x1c00-\x1c37\x1c40-\x1c49" +
            "\x1c4d-\x1c7d\x1cd0-\x1cd2\x1cd4-\x1cf2\x1d00-\x1de6\x1dfc-\x1f15\x1f18-\x1f1d\x1f20-\x1f45\x1f48-\x1f4d\x1f50-\x1f57\x1f59\x1f5b\x1f5d\x1f5f-\x1f7d" +
            "\x1f80-\x1fb4\x1fb6-\x1fbc\x1fbe\x1fc2-\x1fc4\x1fc6-\x1fcc\x1fd0-\x1fd3\x1fd6-\x1fdb\x1fe0-\x1fec\x1ff2-\x1ff4\x1ff6-\x1ffc\x200b-\x200f\x202a-\x202e" +
            "\x203f-\x2040\x2054\x2060-\x2064\x206a-\x206f\x2071\x207f\x2090-\x209c\x20a0-\x20b9\x20d0-\x20dc\x20e1\x20e5-\x20f0\x2102\x2107\x210a-\x2113\x2115" +
            "\x2119-\x211d\x2124\x2126\x2128\x212a-\x212d\x212f-\x2139\x213c-\x213f\x2145-\x2149\x214e\x2160-\x2188\x2c00-\x2c2e\x2c30-\x2c5e\x2c60-\x2ce4" +
            "\x2ceb-\x2cf1\x2d00-\x2d25\x2d30-\x2d65\x2d6f\x2d7f-\x2d96\x2da0-\x2da6\x2da8-\x2dae\x2db0-\x2db6\x2db8-\x2dbe\x2dc0-\x2dc6\x2dc8-\x2dce\x2dd0-\x2dd6" +
            "\x2dd8-\x2dde\x2de0-\x2dff\x2e2f\x3005-\x3007\x3021-\x302f\x3031-\x3035\x3038-\x303c\x3041-\x3096\x3099-\x309a\x309d-\x309f\x30a1-\x30fa\x30fc-\x30ff" +
            "\x3105-\x312d\x3131-\x318e\x31a0-\x31ba\x31f0-\x31ff\x3400-\x4db5\x4e00-\x9fcb\xa000-\xa48c\xa4d0-\xa4fd\xa500-\xa60c\xa610-\xa62b\xa640-\xa66f" +
            "\xa67c-\xa67d\xa67f-\xa697\xa6a0-\xa6f1\xa717-\xa71f\xa722-\xa788\xa78b-\xa78e\xa790-\xa791\xa7a0-\xa7a9\xa7fa-\xa827\xa838\xa840-\xa873\xa880-\xa8c4" +
            "\xa8d0-\xa8d9\xa8e0-\xa8f7\xa8fb\xa900-\xa92d\xa930-\xa953\xa960-\xa97c\xa980-\xa9c0\xa9cf-\xa9d9\xaa00-\xaa36\xaa40-\xaa4d\xaa50-\xaa59\xaa60-\xaa76" +
            "\xaa7a-\xaa7b\xaa80-\xaac2\xaadb-\xaadd\xab01-\xab06\xab09-\xab0e\xab11-\xab16\xab20-\xab26\xab28-\xab2e\xabc0-\xabea\xabec-\xabed\xabf0-\xabf9" +
            "\xac00-\xd7a3\xd7b0-\xd7c6\xd7cb-\xd7fb\xf900-\xfa2d\xfa30-\xfa6d\xfa70-\xfad9\xfb00-\xfb06\xfb13-\xfb17\xfb1d-\xfb28\xfb2a-\xfb36\xfb38-\xfb3c\xfb3e" +
            "\xfb40-\xfb41\xfb43-\xfb44\xfb46-\xfbb1\xfbd3-\xfd3d\xfd50-\xfd8f\xfd92-\xfdc7\xfdf0-\xfdfc\xfe00-\xfe0f\xfe20-\xfe26\xfe33-\xfe34\xfe4d-\xfe4f\xfe69" +
            "\xfe70-\xfe74\xfe76-\xfefc\xfeff\xff04\xff10-\xff19\xff21-\xff3a\xff3f\xff41-\xff5a\xff66-\xffbe\xffc2-\xffc7\xffca-\xffcf\xffd2-\xffd7\xffda-\xffdc" +
            "\xffe0-\xffe1\xffe5-\xffe6\xfff9-\xfffb]*";

        /// <summary>
        /// create a parser matches Java identifier.
        /// </summary>
        /// <returns>parser</returns>
        public static Parser<string> JavaIdentifier()
        {
            return Regex(JavaIdentifierRegex).SelectError(x => "Does not match identifier");
        }

        /// <summary>
        /// Flags of number literal.
        /// </summary>
        [Flags]
        public enum NumberLiteralFlags
        {
            /// <summary>
            /// not use binary and octal.
            /// </summary>
            None = 0,

            /// <summary>
            /// use binary.
            /// </summary>
            Binary = 1,

            /// <summary>
            /// use octal.
            /// </summary>
            Octal = 2
        }

        /// <summary>
        /// create a parser matches number literal.
        /// </summary>
        /// <param name="flags">flags of digits</param>
        /// <returns>parser</returns>
        public static Parser<long> NumberLiteral(NumberLiteralFlags flags)
        {
            var octal = from _1 in Str("0")
                        from a in Regex("[0-7]+").Select(x => Convert.ToInt64(x, 8))
                        select a;
            var hexadecimal = from _1 in Str("0x")
                              from a in Regex("[0-9a-fA-F]+").Select(x => Convert.ToInt64(x, 16))
                              select a;
            var binary = from _1 in Str("0b")
                         from a in Regex("[01]+").Select(x => Convert.ToInt64(x, 2))
                         select a;

            var literal = hexadecimal.Choice(Regex("[0-9]+").Select(x => long.Parse(x)));
            if((flags & NumberLiteralFlags.Binary) == NumberLiteralFlags.Binary)
            {
                literal = binary.Choice(literal);
            }
            if((flags & NumberLiteralFlags.Octal) == NumberLiteralFlags.Octal)
            {
                literal = octal.Choice(literal);
            }

            return from pos in GetPosition()
                   from a in Str("-").Option("")
                   from b in literal.SelectError(x => "Does not match number literal", x => pos)
                   select a == "-" ? -b : b;
        }

        /// <summary>
        /// create a parser matches number literal.
        /// </summary>
        /// <param name="flags">flags of digits</param>
        /// <returns>parser</returns>
        public static Parser<long> NumberLiteral()
        {
            return NumberLiteral(NumberLiteralFlags.Binary | NumberLiteralFlags.Octal);
        }

        /// <summary>
        /// Default escape character function of C#.
        /// </summary>
        /// <param name="x">escape character</param>
        /// <returns></returns>
        public static string CSharpEscapeChar(char x)
        {
            switch (x)
            {
                case '0': return "\0";
                case 'a': return "\a";
                case 'b': return "\b";
                case 'f': return "\f";
                case 'n': return "\n";
                case 'r': return "\r";
                case 't': return "\t";
                case 'v': return "\v";
                case '\'': return "\'";
                case '\"': return "\"";
                case '\\': return "\\";
                default: return null;
            }
        }

        /// <summary>
        /// Default escape character function of C.
        /// </summary>
        /// <param name="x">escape character</param>
        /// <returns></returns>
        public static string CEscapeChar(char x)
        {
            switch (x)
            {
                case '0': return "\0";
                case 'a': return "\a";
                case 'b': return "\b";
                case 'f': return "\f";
                case 'n': return "\n";
                case 'r': return "\r";
                case 't': return "\t";
                case 'v': return "\v";
                case '\'': return "\'";
                case '\"': return "\"";
                case '\\': return "\\";
                case '?': return "?";
                default: return null;
            }
        }

        /// <summary>
        /// Default escape character function of JavaScript.
        /// </summary>
        /// <param name="x">escape character</param>
        /// <returns></returns>
        public static string JSEscapeChar(char x)
        {
            switch (x)
            {
                case '0': return "\0";
                case 'b': return "\b";
                case 'f': return "\f";
                case 'n': return "\n";
                case 'r': return "\r";
                case 't': return "\t";
                case 'v': return "\v";
                case '\'': return "\'";
                case '\"': return "\"";
                case '\\': return "\\";
                default: return null;
            }
        }

        /// <summary>
        /// Default escape character function of Java.
        /// </summary>
        /// <param name="x">escape character</param>
        /// <returns></returns>
        public static string JavaEscapeChar(char x)
        {
            switch (x)
            {
                case 'b': return "\b";
                case 't': return "\t";
                case 'n': return "\n";
                case 'r': return "\r";
                case 'f': return "\f";
                case '\'': return "\'";
                case '\"': return "\"";
                case '\\': return "\\";
                default: return null;
            }
        }

        /// <summary>
        /// Flags of string literal.
        /// </summary>
        [Flags]
        public enum StringLiteralFlags
        {
            None = 0,
            IncludeNewline = 1,
            Octal = 2,
            Hexadecimal = 4,
            Unicode = 8,
            Unicodex = 16,
            JSStyleCodePoint = 32,
            CSharpStyleCodePoint = 64
        }

        private static int HexToInt(string x)
        {
            try
            {
                return Convert.ToInt32(x, 16);
            }
            catch(OverflowException)
            {
                return -1;
            }
        }

        private static string CodeToString(int x)
        {
            StringBuilder builder = new StringBuilder();

            builder.Append((char)x);
            return builder.ToString();
        }

        private static string CodePointToString(int x)
        {
            return (x < 0 || x > 0x10ffff) ? null : char.ConvertFromUtf32(x);
        }

        /// <summary>
        /// creates a parser of string literal.
        /// </summary>
        /// <param name="quoteChar">quote character of string literal</param>
        /// <param name="escapeFunc">function to get escape character to actual character</param>
        /// <param name="flags">flags of parsing string literal</param>
        /// <param name="errorMessage">error message</param>
        /// <returns>parser of matching a string literal</returns>
        public static Parser<string> StringLiteral(char quoteChar,
            Func<char, string> escapeFunc,
            StringLiteralFlags flags,
            string errorMessage)
        {
            string quoteStr = new StringBuilder().Append(quoteChar).ToString();
            string newlineStr = ((flags & StringLiteralFlags.IncludeNewline) == StringLiteralFlags.IncludeNewline) ? "" : "\n";

            var letter = Regex("[^\\\\" + quoteStr + newlineStr + "]+");
            if((flags & StringLiteralFlags.Octal) == StringLiteralFlags.Octal)
            {
                letter = Regex(@"\\[0-3][0-7][0-7]").Select(x => CodeToString(Convert.ToInt32(x.Substring(1), 8))).Choice(letter);
            }
            if ((flags & StringLiteralFlags.Unicodex) == StringLiteralFlags.Unicodex)
            {
                letter = Regex(@"\\x[0-9A-Fa-f]{1,4}").Select(x => CodeToString(Convert.ToInt32(x.Substring(2), 16))).Choice(letter);
            }
            if ((flags & StringLiteralFlags.Hexadecimal) == StringLiteralFlags.Hexadecimal)
            {
                letter = Regex(@"\\x[0-9A-Fa-f][0-9A-Fa-f]").Select(x => CodeToString(Convert.ToInt32(x.Substring(2), 16))).Choice(letter);
            }
            if ((flags & StringLiteralFlags.Unicode) == StringLiteralFlags.Unicode)
            {
                letter = Regex(@"\\u[0-9A-Fa-f]{4,4}").Select(x => CodeToString(Convert.ToInt32(x.Substring(2), 16))).Choice(letter);
            }
            if ((flags & StringLiteralFlags.JSStyleCodePoint) == StringLiteralFlags.JSStyleCodePoint)
            {
                var code = from _1 in Str("\\u{")
                           from a in Regex("[0-9A-Fa-f]+")
                           from _2 in Str("}")
                           select a;
                letter = code.Select(x => CodePointToString(HexToInt(x))).MatchIf(x => x != null).Choice(letter);
            }
            if((flags & StringLiteralFlags.CSharpStyleCodePoint) == StringLiteralFlags.CSharpStyleCodePoint)
            {
                letter = Regex(@"\\U[0-9A-Fa-f]{8,8}").Select(x => CodePointToString(HexToInt(x.Substring(2)))).MatchIf(x => x != null)
                    .Choice(letter);
            }
            letter = Regex(@"\\.").Select(x => escapeFunc(x[1])).MatchIf(x => x != null).Choice(letter);

            var letters = letter.ZeroOrMore((x, y) => x + y, "");

            return (from a in Str(quoteStr)
                    from x in letters
                    from c in Str(quoteStr)
                    select x).SelectError(x => errorMessage);
        }

        /// <summary>
        /// creates a parser of string literal.
        /// </summary>
        /// <param name="quoteChar">quote character of string literal</param>
        /// <param name="escapeFunc">function to get escape character to actual character</param>
        /// <param name="flags">flags of parsing string literal</param>
        /// <returns>parser of matching a string literal</returns>
        public static Parser<string> StringLiteral(char quoteChar,
            Func<char, string> escapeFunc,
            StringLiteralFlags flags)
        {
            return StringLiteral(quoteChar, escapeFunc, flags, "Does not match a string literal");
        }

        /// <summary>
        /// creates a parser of JavaScript string literal.
        /// </summary>
        /// <param name="errorMessage">error message</param>
        /// <returns>parser of matching a string literal</returns>
        public static Parser<string> JSStringLiteral(string errorMessage)
        {
            return StringLiteral('\"',
                JSEscapeChar,
                StringLiteralFlags.Octal | StringLiteralFlags.Hexadecimal | StringLiteralFlags.Unicode | StringLiteralFlags.JSStyleCodePoint,
                errorMessage);
        }

        /// <summary>
        /// creates a parser of JavaScript string literal.
        /// </summary>
        /// <returns>parser of matching a string literal</returns>
        public static Parser<string> JSStringLiteral()
        {
            return JSStringLiteral("Does not match a string literal");
        }

        /// <summary>
        /// creates a parser of Java string literal.
        /// </summary>
        /// <param name="errorMessage">error message</param>
        /// <returns>parser of matching a string literal</returns>
        public static Parser<string> JavaStringLiteral(string errorMessage)
        {
            return StringLiteral('\"',
                JavaEscapeChar,
                StringLiteralFlags.Octal | StringLiteralFlags.Unicode,
                errorMessage);
        }

        /// <summary>
        /// creates a parser of Java string literal.
        /// </summary>
        /// <returns>parser of matching a string literal</returns>
        public static Parser<string> JavaStringLiteral()
        {
            return JavaStringLiteral("Does not match a string literal");
        }

        /// <summary>
        /// creates a parser of C string literal.
        /// </summary>
        /// <param name="errorMessage">error message</param>
        /// <returns>parser of matching a string literal</returns>
        public static Parser<string> CStringLiteral(string errorMessage)
        {
            return StringLiteral('\"',
                CEscapeChar,
                StringLiteralFlags.Octal | StringLiteralFlags.Hexadecimal,
                errorMessage);
        }

        /// <summary>
        /// creates a parser of C string literal.
        /// </summary>
        /// <returns>parser of matching a string literal</returns>
        public static Parser<string> CStringLiteral()
        {
            return CStringLiteral("Does not match a string literal");
        }

        /// <summary>
        /// creates a parser of C# string literal.
        /// </summary>
        /// <param name="errorMessage">error message</param>
        /// <returns>parser of matching a string literal</returns>
        public static Parser<string> CSharpStringLiteral(string errorMessage)
        {
            return StringLiteral('\"',
                CSharpEscapeChar,
                StringLiteralFlags.Unicodex | StringLiteralFlags.Unicode | StringLiteralFlags.CSharpStyleCodePoint,
                errorMessage);
        }

        /// <summary>
        /// creates a parser of C# string literal.
        /// </summary>
        /// <returns>parser of matching a string literal</returns>
        public static Parser<string> CSharpStringLiteral()
        {
            return CSharpStringLiteral("Does not match a string literal");
        }
    }
}
