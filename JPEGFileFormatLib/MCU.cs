using JPEGFileFormatLib.Markers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace JPEGFileFormatLib
{
    /// <summary>
    /// Minimum coded unit
    /// https://www.impulseadventure.com/photo/jpeg-minimum-coded-unit.html
    /// </summary>
    internal class MCU
    {
        Dictionary<string, Component> Components = new Dictionary<string, Component>();

        internal void ReadData(DHT lastDHT, StringBuilder binaryStringBuilder)
        {
            AddComponent("Y", 0, lastDHT, binaryStringBuilder);
            AddComponent("Cb", 1, lastDHT, binaryStringBuilder);
            AddComponent("Cr", 1, lastDHT, binaryStringBuilder);
        }

        private void AddComponent(string name, int componentId, DHT lastDHT, StringBuilder binaryStringBuilder)
        {
            Component newComponent = new Component(componentId);
            newComponent.ReadDC(lastDHT, binaryStringBuilder);
            newComponent.ReadAC(lastDHT, binaryStringBuilder);
            Components.Add(name, newComponent);
        }

        internal class Component
        {
            int dc;
            readonly List<int> ac = new List<int>();
            readonly int componentId;

            public Component(int componentId)
            {
                this.componentId = componentId;
            }

            internal void ReadDC(DHT lastDHT, StringBuilder binaryStringBuilder)
            {
                DHT.DHTStruct dcTable = lastDHT.Tables.First(t => t.TableType == DHT.HuffmanTableType.DC && t.NumberOfHuffmanTable == componentId);

                int i = 1;
                while (!dcTable.bitMaps.ContainsKey(binaryStringBuilder.ToString().Substring(0, i)))
                    i++;

                int additionalBitsToConvert = dcTable.bitMaps[binaryStringBuilder.ToString().Substring(0, i)];
                if (additionalBitsToConvert == 0)
                {
                    binaryStringBuilder.Remove(0, i + additionalBitsToConvert);
                    dc = 0;
                    return;
                }
                string dcCodeValue = binaryStringBuilder.ToString().Substring(i, additionalBitsToConvert);
                binaryStringBuilder.Remove(0, i + additionalBitsToConvert);
                if (dcCodeValue.StartsWith("0"))
                {
                    dcCodeValue = new string(dcCodeValue.Select(d => d == '0' ? '1' : '0').ToArray());
                    dc = -Convert.ToInt32(dcCodeValue, 2);
                }
                else
                    dc = Convert.ToInt32(dcCodeValue, 2);
            }

            internal void ReadAC(DHT lastDHT, StringBuilder binaryStringBuilder)
            {
                int currentAC = 0;
                //int coeffRemoved = 0;
                do
                {
                    DHT.DHTStruct acTable = lastDHT.Tables.First(t => t.TableType == DHT.HuffmanTableType.AC && t.NumberOfHuffmanTable == componentId);

                    int i = 1;
                    while (!acTable.bitMaps.ContainsKey(binaryStringBuilder.ToString().Substring(0, i)))
                        i++;

                    int additionalBitsToConvert = acTable.bitMaps[binaryStringBuilder.ToString().Substring(0, i)];
                    while (additionalBitsToConvert > 16)
                    {
                        additionalBitsToConvert -= 16;
                        ac.Add(0);
                        //coeffRemoved++;
                    }
                    if (additionalBitsToConvert == 0)
                    {
                        ac.Add(0);
                        binaryStringBuilder.Remove(0, i + additionalBitsToConvert);
                        break;
                    }
                    else
                    {
                        string acCodeValue = binaryStringBuilder.ToString().Substring(i, additionalBitsToConvert);
                        if (acCodeValue.StartsWith("0"))
                        {
                            acCodeValue = new string(acCodeValue.Select(d => d == '0' ? '1' : '0').ToArray());
                            currentAC = -Convert.ToInt32(acCodeValue, 2);
                        }
                        else
                            currentAC = Convert.ToInt32(acCodeValue, 2);
                        binaryStringBuilder.Remove(0, i + additionalBitsToConvert);
                    }
                    //currentAC = acTable.bitMaps[binaryStringBuilder.ToString().Substring(0, i)];
                    //if (currentAC != 0)
                    ac.Add(currentAC);

                    if (ac.Count >= 63)
                        break;
                    //binaryStringBuilder.Remove(0, i);
                } while (currentAC != 0);
            }
        }
    }
}
