using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JPEGFileFormatLib.Markers
{
    /// <summary>
    /// Photoshop or Adobe_CM tag. More info : https://sno.phy.queensu.ca/~phil/exiftool/TagNames/Photoshop.html
    /// </summary>
    internal class APP13 : JpegMarkerBase
    {
        public string Tag { get; set; }
        public byte AdobeCMType { get; set; }
        public List<APP13Block> Blocks { get; set; } = new List<APP13Block>();

        internal APP13() : base(JpegMarker.APP13)
        {
        }

        public override void ReadExtensionData(BinaryReaderFlexiEndian reader)
        {
            while (reader.PeekChar() != 0)
                Tag += reader.ReadChar();

            byte extraByte = reader.ReadByte(); //Discard null terminator

            if (Tag.Equals("Adobe_CM", StringComparison.OrdinalIgnoreCase))
            {
                AdobeCMType = reader.ReadByte();
            }
            else
            {
                while (reader.BaseStream.Position != reader.BaseStream.Length)
                    Blocks.Add(new APP13Block(reader));
            }
        }

        internal class APP13Block
        {
            readonly string tag;
            readonly PhotoshopTag tagMarker;
            readonly byte nameLength;
            readonly byte padding;
            readonly uint size;
            readonly byte[] data;
            internal APP13Block(BinaryReaderFlexiEndian reader)
            {
                for (int i = 0; i < 4; i++)
                    tag += reader.ReadChar();

                tagMarker = (PhotoshopTag)reader.ReadUInt16();
                nameLength = reader.ReadByte();
                padding = reader.ReadByte();
                size = reader.ReadUInt32();

                data = reader.ReadBytes((int)size);

                while (reader.PeekChar() == 0x00)
                    reader.ReadByte();
            }

            /// <summary>
            /// Source : https://sno.phy.queensu.ca/~phil/exiftool/TagNames/Photoshop.html
            /// </summary>
            internal enum PhotoshopTag
            {
                Photoshop2Info = 0x03e8,
                MacintoshPrintInfo = 0x03e9,
                XMLData = 0x03ea,
                Photoshop2ColorTable = 0x03eb,
                ResolutionInfo = 0x03ed,
                AlphaChannelsNames = 0x03ee,
                DisplayInfo = 0x03ef,
                PStringCaption = 0x03f0,
                BorderInformation = 0x03f1,
                BackgroundColor = 0x03f2,
                PrintFlags = 0x03f3,
                BW_HalftoningInfo = 0x03f4,
                ColorHalftoningInfo = 0x03f5,
                DuotoneHalftoningInfo = 0x03f6,
                BW_TransferFunc = 0x03f7,
                ColorTransferFuncs = 0x03f8,
                DuotoneTransferFuncs = 0x03f9,
                DuotoneImageInfo = 0x03fa,
                EffectiveBW = 0x03fb,
                ObsoletePhotoshopTag1 = 0x03fc,
                EPSOptions = 0x03fd,
                QuickMaskInfo = 0x03fe,
                ObsoletePhotoshopTag2 = 0x03ff,
                TargetLayerID = 0x0400,
                WorkingPath = 0x0401,
                LayersGroupInfo = 0x0402,
                ObsoletePhotoshopTag3 = 0x0403,
                IPTCData = 0x0404,
                RawImageMode = 0x0405,
                JPEG_Quality = 0x0406,
                GridGuidesInfo = 0x0408,
                PhotoshopBGRThumbnail = 0x0409,
                CopyrightFlag = 0x040a,
                URL = 0x040b,
                PhotoshopThumbnail = 0x040c,
                GlobalAngle = 0x040d,
                ColorSamplersResource = 0x040e,
                ICC_Profile = 0x040f,
                Watermark = 0x0410,
                ICC_Untagged = 0x0411,
                EffectsVisible = 0x0412,
                SpotHalftone = 0x0413,
                IDsBaseValue = 0x0414,
                UnicodeAlphaNames = 0x0415,
                IndexedColorTableCount = 0x0416,
                TransparentIndex = 0x0417,
                GlobalAltitude = 0x0419,
                SliceInfo = 0x041a,
                WorkflowURL = 0x041b,
                JumpToXPEP = 0x041c,
                AlphaIdentifiers = 0x041d,
                URL_List = 0x041e,
                VersionInfo = 0x0421,
                EXIFInfo = 0x0422,
                ExifInfo2 = 0x0423,
                XMP = 0x0424,
                IPTCDigest = 0x0425,
                PrintScaleInfo = 0x0426,
                PixelInfo = 0x0428,
                LayerComps = 0x0429,
                AlternateDuotoneColors = 0x042a,
                AlternateSpotColors = 0x042b,
                LayerSelectionIDs = 0x042d,
                HDRToningInfo = 0x042e,
                PrintInfo = 0x042f,
                LayerGroupsEnabledID = 0x0430,
                ColorSamplersResource2 = 0x0431,
                MeasurementScale = 0x0432,
                TimelineInfo = 0x0433,
                SheetDisclosure = 0x0434,
                DisplayInfo2 = 0x0435,
                OnionSkins = 0x0436,
                CountInfo = 0x0438,
                PrintInfo2 = 0x043a,
                PrintStyle = 0x043b,
                MacintoshNSPrintInfo = 0x043c,
                WindowsDEVMODE = 0x043d,
                AutoSaveFilePath = 0x043e,
                AutoSaveFormat = 0x043f,
                PathSelectionState = 0x0440,
                ClippingPathName = 0x0bb7,
                OriginPathInfo = 0x0bb8,
                ImageReadyVariables = 0x1b58,
                ImageReadyDataSets = 0x1b59,
                LightroomWorkflow = 0x1f40,
                PrintFlagsInfo = 0x2710,
            }
        }
    }
}
