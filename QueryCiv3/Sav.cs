using System;
using System.Collections.Generic;
using QueryCiv3.Sav;

namespace QueryCiv3 {
	public unsafe class SavData {
		public BiqData Bic;
		public Civ3File Sav;

		public byte* scan;

		public GAME Game;
		public WRLD Wrld;
		public TILE[] Tile;
		public CONT[] Cont;
		public LEAD[] Lead;
		public AIBS[] Aibs;
		public OUTP[] Outp;
		public VLOC[] Vloc;
		public RADT[] Radt;
		public CITY[] City;
		public PEER Peer;
		public PALV[] Palv;
		public DATE[] Date = new DATE[3];
		public PLGI Plgi;
		public CNSL Cnsl;
		public TUTR Tutr;
		public FAXX Faxx;
		public HIST Hist;
		public UNIT[] Unit;
		public IDLS[] Idls;
		public CLNY[] Clny;

		public int[] CitiesPerContinent;
		public IntBitmap[] KnownTechFlags;
		public int[] GreatWonderCityIDs;
		public bool[] GreatWondersBuilt;
		public int[] BuildingData1;
		public int[] BuildingData2;
		public int[] PrototypeStrategy1;
		public int[] PrototypeStrategy2;
		public int[] TechData;

		public int[] ResourceCounts;

		private int DateIndex = 0;

		// LEAD section logic (blegh!):
		public LEAD_LEAD[][] ReputationRelationship;
		public LEAD_LEAD_Diplomacy[,][] LeadLeadDiplomacy;
		public short[][] LeadBldgCount;
		public short[][] LeadBldgInConstruction;
		public short[][] LeadBldgData;
		public int[][] LeadBldgSmallWonderCity;
		public bool[][] LeadBldgSmallWonderBuilt;
		public short[][] LeadPrtoCount;
		public short[][] LeadPrtoInConstruction;
		public short[][] LeadPrtoData;
		public short[][] LeadSpaceshipParts;
		public LEAD_GOOD_LEAD[,][] LeadGoodLead;
		public bool[][] LeadGoodAvailable;
		public int[][] LeadContCityCount;
		public int[][] LeadTechQueue;

		private const int LEAD_COUNT = 32; // should always be 32 in Conquests savs
		private const int LEAD_LEN_1 = 412;
		private const int LEAD_LEN_2 = 2696;
		private const int LEAD_LEN_3 = 108;
		private const int LEAD_LEN_4 = 292;

		public RPLT[] Rplt;
		public RPLE[][] RpltRple;
		public string[][] RpltRpleDescription;

		public CTZN[][] CityCtzn;
		public CITY_Building[][] CityBuilding;

		private int CityIndex = 0;
		private const int VALID_CITY_LENGTH = 136;
		private const int CITY_LEN_1 = 556;
		private const int CITY_LEN_2 = 12;
		private const int CITY_LEN_3 = 140;

		public Turn[] HistTurn;
		public int[][] TurnCiv;
		public int[][] TurnPower;
		public int[][] TurnScore;
		public int[][] TurnCulture;
		public int[][] TurnVP;

		private const int BIQ_SECTION_START = 562;

		public SavData(byte[] savBytes, byte[] biqBytes) {
			Bic = new BiqData(biqBytes);
			Load(savBytes);
		}

		public unsafe void Copy<T>(ref T data, int length = -1, int offset = 0) where T : unmanaged {
			if (length == -1) {
				length = sizeof(T);
			}

			fixed (void* destPtr = &data) {
				Buffer.MemoryCopy(scan, (byte*)destPtr + offset, length, length);
			}
			scan += length;
		}

		public unsafe void CopyArray<T>(ref T[] data, int length) where T : unmanaged {
			data = new T[length];
			int dataLength = length * sizeof(T);

			fixed (void* destPtr = data) {
				Buffer.MemoryCopy(scan, destPtr, dataLength, dataLength);
			}
			scan += dataLength;
		}

		public unsafe void Load(byte[] savBytes) {
			Sav = new Civ3File(savBytes);
			// Load in any biq sections contained in Sav file, overwriting existing biq sections:
			int BiqSectionLength = Sav.ReadInt32(38);
			Bic.Load(Sav.GetBytes(BIQ_SECTION_START, BiqSectionLength));

			fixed (byte* bytePtr = savBytes) {
				int* header;
				scan = bytePtr + BIQ_SECTION_START + BiqSectionLength;
				byte* end = bytePtr + savBytes.Length;

				while (scan < end) {
					header = (int*)scan;

					// Every header in civ 3 files is exactly 4-chars long, which means they can be represented as 32-bit integers instead of strings
					// Switching off of these hex values is substantially faster than string switching, but comes at the expense of readability
					switch (*header) {
						case 0x454d4147: // GAME
							Copy(ref Game);
							CopyArray(ref CitiesPerContinent, Game.NumberOfContinents);
							CopyArray(ref KnownTechFlags, Bic.Tech.Length);
							CopyArray(ref GreatWonderCityIDs, Bic.Bldg.Length);
							CopyArray(ref GreatWondersBuilt, Bic.Bldg.Length);
							CopyArray(ref BuildingData1, Bic.Bldg.Length);
							CopyArray(ref BuildingData2, Bic.Bldg.Length);
							CopyArray(ref PrototypeStrategy1, Bic.Prto.Length);
							CopyArray(ref PrototypeStrategy2, Bic.Prto.Length);
							CopyArray(ref TechData, Bic.Tech.Length);

							// Instantiate City arrays after getting city count
							// Normally it'd be more straightforward to do this at the city case statement itself, but city sections are unique
							//   in that they can have many "dirty" sections which means the case statement might be encountered multiple times
							City = new CITY[Game.NumberOfCities];
							CityCtzn = new CTZN[Game.NumberOfCities][];
							CityBuilding = new CITY_Building[Game.NumberOfCities][];

							break;
						case 0x444c5257: // WRLD
							Copy(ref Wrld);
							break;
						case 0x454c4954: // TILE
							CopyArray(ref Tile, Wrld.Width * Wrld.Height / 2);
							break;
						case 0x544e4f43: // CONT
							CopyArray(ref Cont, Game.NumberOfContinents);
							CopyArray(ref ResourceCounts, Bic.Good.Length);
							break;
						case 0x4441454c: // LEAD
							Lead = new LEAD[LEAD_COUNT];
							ReputationRelationship = new LEAD_LEAD[LEAD_COUNT][];
							LeadLeadDiplomacy = new LEAD_LEAD_Diplomacy[LEAD_COUNT, LEAD_COUNT][];
							LeadBldgCount = new short[LEAD_COUNT][];
							LeadBldgInConstruction = new short[LEAD_COUNT][];
							LeadBldgData = new short[LEAD_COUNT][];
							LeadBldgSmallWonderCity = new int[LEAD_COUNT][];
							LeadBldgSmallWonderBuilt = new bool[LEAD_COUNT][];
							LeadPrtoCount = new short[LEAD_COUNT][];
							LeadPrtoInConstruction = new short[LEAD_COUNT][];
							LeadPrtoData = new short[LEAD_COUNT][];
							LeadSpaceshipParts = new short[LEAD_COUNT][];
							LeadGoodLead = new LEAD_GOOD_LEAD[LEAD_COUNT, Bic.Good.Length][];
							LeadGoodAvailable = new bool[LEAD_COUNT][];
							LeadContCityCount = new int[LEAD_COUNT][];
							LeadTechQueue = new int[LEAD_COUNT][];

							for (int i = 0; i < LEAD_COUNT; i++) {
								Copy(ref Lead[i], LEAD_LEN_1);
								CopyArray(ref ReputationRelationship[i], LEAD_COUNT);
								Copy(ref Lead[i], LEAD_LEN_2, LEAD_LEN_1);

								for (int j = 0; j < LEAD_COUNT; j++) {
									// Number of diplomacy entries stored as integer, so get pointer to that and then skip over those 4 bytes:
									header = (int*)scan;
									scan += 4;
									CopyArray(ref LeadLeadDiplomacy[i, j], *header);
								}

								if (Lead[i].RaceID != -1) { // if an actual leader
									CopyArray(ref LeadBldgCount[i], Bic.Bldg.Length);
									CopyArray(ref LeadBldgInConstruction[i], Bic.Bldg.Length);
									CopyArray(ref LeadBldgData[i], Bic.Bldg.Length);
									CopyArray(ref LeadBldgSmallWonderCity[i], Bic.Bldg.Length);
									CopyArray(ref LeadBldgSmallWonderBuilt[i], Bic.Bldg.Length);
									CopyArray(ref LeadPrtoCount[i], Bic.Prto.Length);
									CopyArray(ref LeadPrtoInConstruction[i], Bic.Prto.Length);
									CopyArray(ref LeadPrtoData[i], Bic.Prto.Length);
									CopyArray(ref LeadSpaceshipParts[i], Bic.Rule[0].NumberOfSpaceshipParts);
									for (int j = 0; j < Bic.Good.Length; j++) {
										CopyArray(ref LeadGoodLead[i, j], LEAD_COUNT);
									}
									CopyArray(ref LeadGoodAvailable[i], Bic.Good.Length);
									scan += Wrld.ContinentCount * 16; // 16 bytes of unknown data per continent
									CopyArray(ref LeadContCityCount[i], Wrld.ContinentCount);
								}

								Copy(ref Lead[i], LEAD_LEN_3, LEAD_LEN_1 + LEAD_LEN_2);
								CopyArray(ref LeadTechQueue[i], Lead[i].ScienceQueueSize);
								Copy(ref Lead[i], LEAD_LEN_4, LEAD_LEN_1 + LEAD_LEN_2 + LEAD_LEN_3);
							}
							break;
						case 0x534c5052: // RPLS
										 // RPLS just consists of the 4-byte header and a 32-bit integer for the number of turns (RPLTs)
										 // Because it's so simple, don't even both memory-copying into a struct for it and just get the RPLT length
							header = (int*)(scan + 4);
							int rpltLength = *header;
							scan += 8; // skip RPLS header and length integer

							Rplt = new RPLT[rpltLength];
							RpltRple = new RPLE[rpltLength][];
							RpltRpleDescription = new string[rpltLength][];
							const int MAX_STRING_LENGTH = 1024; // surely no event string is longer than 1024 characters?
							byte[] stringBuffer = new byte[MAX_STRING_LENGTH];

							fixed (byte* strPtr = stringBuffer) {
								for (int i = 0; i < rpltLength; i++) {
									Copy(ref Rplt[i]);
									int rpleLength = Rplt[i].EventCount;

									RpltRple[i] = new RPLE[rpleLength];
									RpltRpleDescription[i] = new string[rpleLength];
									for (int j = 0; j < rpleLength; j++) {
										Copy(ref RpltRple[i][j]);
										// Retrieve null-terminated string:
										int counter = 0;
										while (scan[counter++] != 0) { } // Keep incrementing until null character reached
										Buffer.MemoryCopy(scan, strPtr, MAX_STRING_LENGTH, counter);
										// Calling Util.GetString with the full buffer works, but is inefficient. Optimizing this is a TODO
										RpltRpleDescription[i][j] = Util.GetString(stringBuffer);
										scan += counter;
									}
								}
							}

							break;
						case 0x53424941: // AIBS
							CopyArray(ref Aibs, Game.NumberOfAirbases);
							break;
						case 0x5054554f: // OUTP
							CopyArray(ref Outp, Game.NumberOfOutposts);
							break;
						case 0x434f4c56: // VLOC
							CopyArray(ref Vloc, Game.NumberOfVPLocations);
							break;
						case 0x54444152: // RADT
							CopyArray(ref Radt, Game.NumberOfRadarTowers);
							break;
						case 0x59544943: // CITY
										 // Sav files contain many "bad" City headers. In fact, there are more bad ones than valid ones
										 // The purpose behind these headers is yet to be determined, but for now, they can be skipped
							if (scan[4] == VALID_CITY_LENGTH) {
								Copy(ref City[CityIndex], CITY_LEN_1);
								CopyArray(ref CityCtzn[CityIndex], City[CityIndex].Popd.CitizenCount);
								Copy(ref City[CityIndex], CITY_LEN_2, CITY_LEN_1);
								CopyArray(ref CityBuilding[CityIndex], City[CityIndex].Binf.BuildingCount);
								Copy(ref City[CityIndex], CITY_LEN_3, CITY_LEN_1 + CITY_LEN_2);
								CityIndex++;
							} else {
								scan = scan + scan[4] + 8; // Skip ahead header length (4) + length integer length (4) + length integer (scan[4])
							}

							break;
						case 0x52454550: // PEER
							Copy(ref Peer);
							break;
						case 0x564c4150: // PALV
							CopyArray(ref Palv, LEAD_COUNT);
							break;
						case 0x45544144: // DATE
							Copy(ref Date[DateIndex++]);
							break;
						case 0x49474c50: // PLGI
							Copy(ref Plgi);
							break;
						case 0x4c534e43: // CNSL
							Copy(ref Cnsl);
							break;
						case 0x52545554: // TUTR
							Copy(ref Tutr);
							break;
						case 0x58584146: // FAXX
							Copy(ref Faxx);
							break;
						case 0x54534948: // HIST
							Copy(ref Hist);
							HistTurn = new Turn[Hist.TurnCount];
							TurnCiv = new int[Hist.TurnCount][];
							TurnPower = new int[Hist.TurnCount][];
							TurnScore = new int[Hist.TurnCount][];
							TurnCulture = new int[Hist.TurnCount][];
							TurnVP = new int[Hist.TurnCount][];

							// Histogram tracks Power, Score, Culture, and optionally Victory Points if that victory condition is enabled:
							for (int i = 0; i < Hist.TurnCount; i++) {
								Copy(ref HistTurn[i]);
								int CivCount = HistTurn[i].RemainingCivs;
								CopyArray(ref TurnCiv[i], CivCount);
								CopyArray(ref TurnPower[i], CivCount);
								CopyArray(ref TurnScore[i], CivCount);
								CopyArray(ref TurnCulture[i], CivCount);
								if (Game.VictoryLocations) {
									CopyArray(ref TurnVP[i], CivCount);
								}
							}
							break;
						case 0x54494e55: // UNIT
										 // Because most units have IDLS sections, it's easier to keep the array lengths the same and accept
										 // that some indexes of Idls will be unused
							Unit = new UNIT[Game.NumberOfUnits];
							Idls = new IDLS[Game.NumberOfUnits];

							for (int i = 0; i < Game.NumberOfUnits; i++) {
								Copy(ref Unit[i]);
								if (Unit[i].HasIDLSSection) {
									Copy(ref Idls[i]);
								}
							}

							break;
						case 0x47505443: // CTPG
										 // It's unclear what CTPG does, so for now, give it the invalid CITY treatment
							scan = scan + scan[4] + 8;
							break;
						case 0x594e4c43: // CLNY
							CopyArray(ref Clny, Game.NumberOfColonies);
							break;
						default:
							// There are 3 places in the Sav files where an inexplicable but consistent gap between sections exists
							// In any other case where a header isn't encounterd, we'll throw an error because something has gone wrong in the read
							// But for these 3, I guess just skip them for now...
							// Thoroughly magic
							if (header[2] == 0x4c534e43) {
								scan += 8;
							} else if (header[64] == 0x564c4150) {
								scan += 256;
							} else if (header[1] == 0x52454550) {
								scan += 4;
							} else {
								throw new Exception("An error occured while parsing the SAV file because no header was found where one was expected.");
							}
							break;
					}
				}
			}
		}
	}
}
