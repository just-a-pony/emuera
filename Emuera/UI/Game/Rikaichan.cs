﻿using MinorShift.Emuera.Forms;
using MinorShift.Emuera.Runtime.Config;
using MinorShift.Emuera.UI.Game;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO; //for File
using System.Text;
using System.Windows.Forms; //for TextRenderer

namespace MinorShift.Emuera.GameView;

partial class Rikaichan
{
	public bool enabled = true;
	public ConsoleStyledString css;
	public Point point;
	public int curLineY = -1;
	public string laststr;
	public int strpos = -1;
	public int laststrpos = -1;
	public string output;

	public byte[] edict;
	public byte[] edictind;
	public Encoding eucjp = Encoding.GetEncoding(20932);

	public bool hidden = true;

	public List<int> searchResults = new(0x20);
	public int indexLast;

	public int longestResult;

	SolidBrush blueBrush;

	public void ReceiveIndex(byte[] edictind)
	{
		this.edictind = edictind;
	}

	private void Init()
	{
		enabled = Config.RikaiEnabled;
		if (!enabled) return;

		var rikaiFilename = Program.ExeDir + Config.RikaiFilename;

		if (!File.Exists(rikaiFilename))
		{
			MessageBox.Show($"{Config.RikaiFilename} not found, rikaichan can't work without that");
			// You need jmdict in edict format, not edict2 or xml, just edict. For now.
			enabled = false;
			return;
		}

		edict = File.ReadAllBytes(rikaiFilename);




		if (File.Exists(rikaiFilename + ".ind"))
		{
			edictind = File.ReadAllBytes(rikaiFilename + ".ind");
			return;
		}
		else
		{
			var dialog = new RikaiDialog(edict, ReceiveIndex);
			dialog.Show();
		}


	}

	private static void Autogenerate()
	{
		using (var reader = new StreamReader("RikaichanAutogenerated.template.cs"))
		{
			using (var writer = new StreamWriter("RikaichanAutogenerated.cs"))
			{
				string line;
				while ((line = reader.ReadLine()) != null)
				{
					if (line == "//{{DeinflectRuleData}}")
					{
						//writer.WriteLine("//SUUUUUUUUUUUUUP");
						using (var reader2 = new StreamReader("deinflect.ts"))
						{
							string line2;
							while ((line2 = reader2.ReadLine()) != "const deinflectRuleData: Array<[string, string, number, number]> = [")
							{
								if (line2 == null) throw new Exception();
							}
							string from, to, type, reason;
							while ((line2 = reader2.ReadLine()) != null)
							{
								if (line2 == "  [")
								{
									{
										line2 = reader2.ReadLine();
										if (line2 == null) throw new Exception();
										int from_start, from_end;
										for (from_start = 0; from_start < line2.Length; from_start++)
										{
											if (line2[from_start] == '\'')
											{
												from_start++;
												break;
											}
										}
										if (from_start >= line2.Length) throw new Exception();
										for (from_end = from_start; from_end < line2.Length; from_end++)
										{
											if (line2[from_end] == '\'')
											{
												break;
											}
										}
										if (from_end >= line2.Length) throw new Exception();
										from = line2.Substring(from_start, from_end - from_start);
									}

									{
										line2 = reader2.ReadLine();
										if (line2 == null) throw new Exception();
										int from_start, from_end;
										for (from_start = 0; from_start < line2.Length; from_start++)
										{
											if (line2[from_start] == '\'')
											{
												from_start++;
												break;
											}
										}
										if (from_start >= line2.Length) throw new Exception();
										for (from_end = from_start; from_end < line2.Length; from_end++)
										{
											if (line2[from_end] == '\'')
											{
												break;
											}
										}
										if (from_end >= line2.Length) throw new Exception();
										to = line2.Substring(from_start, from_end - from_start);
									}

									{
										line2 = reader2.ReadLine();
										if (line2 == null) throw new Exception();
										int from_start, from_end;
										for (from_start = 0; from_start < line2.Length; from_start++)
										{
											if (line2[from_start] != ' ')
											{
												break;
											}
										}
										if (from_start >= line2.Length) throw new Exception();
										for (from_end = from_start; from_end < line2.Length; from_end++)
										{
											if (line2[from_end] == ',')
											{
												break;
											}
										}
										if (from_end >= line2.Length) throw new Exception();
										type = line2.Substring(from_start, from_end - from_start);
									}

									{
										line2 = reader2.ReadLine();
										if (line2 == null) throw new Exception();
										int from_start, from_end;
										for (from_start = 0; from_start < line2.Length; from_start++)
										{
											if (line2[from_start] != ' ')
											{
												break;
											}
										}
										if (from_start >= line2.Length) throw new Exception();
										for (from_end = from_start; from_end < line2.Length; from_end++)
										{
											if (line2[from_end] == ',')
											{
												break;
											}
										}
										if (from_end >= line2.Length) throw new Exception();
										reason = line2.Substring(from_start, from_end - from_start);
									}
									writer.WriteLine("\t\t\tnext = new DeinflectRule()");
									writer.WriteLine("\t\t\t{");
									writer.WriteLine("\t\t\t\tfrom = eucjp.GetBytes(\"" + from + "\"),");
									writer.WriteLine("\t\t\t\tto = eucjp.GetBytes(\"" + to + "\"),");
									writer.WriteLine("\t\t\t\ttype = " + type + ",");
									writer.WriteLine("\t\t\t\treason = " + reason + ",");
									writer.WriteLine("\t\t\t};");
									writer.WriteLine("\t\t\tlist.Add(next);");
									line2 = reader2.ReadLine();
									if (line2 != "  ],") throw new Exception();
								}
								else if (line2 == "];")
								{
									break;
								}
								else if (line2 == null)
								{
									throw new Exception();
								}
								else
								{
									int s, e;
									for (s = 0; s < line2.Length; s++)
									{
										if (line2[s] == '\'')
										{
											s++;
											break;
										}
									}
									if (s >= line2.Length) throw new Exception();
									for (e = s; e < line2.Length; e++)
									{
										if (line2[e] == '\'')
										{
											break;
										}
									}
									if (e >= line2.Length) throw new Exception();
									from = line2.Substring(s, e - s);

									for (s = e + 1; s < line2.Length; s++)
									{
										if (line2[s] == '\'')
										{
											s++;
											break;
										}
									}
									if (s >= line2.Length) throw new Exception();
									for (e = s; e < line2.Length; e++)
									{
										if (line2[e] == '\'')
										{
											break;
										}
									}
									if (e >= line2.Length) throw new Exception();
									to = line2.Substring(s, e - s);

									s = e + 1;
									if (line2[s++] != ',') throw new Exception();
									if (line2[s++] != ' ') throw new Exception();
									if (line2[s] < '0' || line2[s] > '9') throw new Exception();
									for (e = s + 1; e < line2.Length; e++)
									{
										if (line2[e] == ',')
										{
											break;
										}
									}
									if (e >= line2.Length) throw new Exception();
									type = line2.Substring(s, e - s);

									s = e;
									if (line2[s++] != ',') throw new Exception();
									if (line2[s++] != ' ') throw new Exception();
									for (e = s + 1; e < line2.Length; e++)
									{
										if (line2[e] == ']')
										{
											break;
										}
									}
									if (e >= line2.Length) throw new Exception();
									reason = line2.Substring(s, e - s);

									writer.WriteLine("\t\t\tnext = new DeinflectRule()");
									writer.WriteLine("\t\t\t{");
									writer.WriteLine("\t\t\t\tfrom = eucjp.GetBytes(\"" + from + "\"),");
									writer.WriteLine("\t\t\t\tto = eucjp.GetBytes(\"" + to + "\"),");
									writer.WriteLine("\t\t\t\ttype = " + type + ",");
									writer.WriteLine("\t\t\t\treason = " + reason + ",");
									writer.WriteLine("\t\t\t};");
									writer.WriteLine("\t\t\tlist.Add(next);");
								}
							}
						}
					}
					else if (line == "//{{DeinflectReason}}")
					{
						using (var reader2 = new StreamReader("deinflect.ts"))
						{
							string line2;
							while ((line2 = reader2.ReadLine()) != "  PolitePastNegative,")
							{
								if (line2 == null) throw new Exception();
							}
							while ((line2 = reader2.ReadLine()) != null)
							{
								if (line2 == "}")
								{
									break;
								}

								line2 = line2.Substring(2);
								//line2 = line2.Substring(0, line2.Length - 1);
								line2 = "\t\t" + line2;
								writer.WriteLine(line2);
							}
						}
					}
					else if (line == "//{{deinflectReasonStrings}}")
					{
						using (var reader2 = new StreamReader("deinflect.ts"))
						{
							string line2;
							while ((line2 = reader2.ReadLine()) != "export const enum DeinflectReason {")
							{
								if (line2 == null) throw new Exception();
							}
							while ((line2 = reader2.ReadLine()) != null)
							{
								if (line2 == "}")
								{
									break;
								}

								line2 = line2.Substring(2);
								line2 = line2.Substring(0, line2.Length - 1);
								writer.WriteLine("\t\t\t\"" + line2 + "\",");
							}
						}
					}
					else
					{
						writer.WriteLine(line);
					}
				}
			}
		}
	}

	private bool SearchFirst()
	{
		searchResults.Clear();
		longestResult = 0;

		if (edictind == null) return true;

		var search = eucjp.GetBytes(output.Substring(0, 1));
		int start, end, cur, curprev;
		start = 0;
		end = edictind.Length;
		curprev = cur = (start + end) / 2;
		goto search_to_right;

	search_again:
		curprev = cur = (start + end) / 2;

	search_to_right:
		if (cur >= end)
		{
			cur = curprev;
			goto search_to_left;
		}
		if (edictind[cur] != 0)
		{
			cur++;
			goto search_to_right;
		}
		cur++;
		for (int i = 0; i < search.Length; i++)
		{
			if (search[i] < edictind[cur + i])
			{
				if (end == cur) goto search_to_left; //LATER: probably can be done better
				end = cur;
				goto search_again;
			}
			else if (search[i] > edictind[cur + i])
			{
				start = cur;
				goto search_again;
			}
		}
		goto found;

	search_to_left:

		cur--;

		if (cur <= start) goto not_found;

		if (edictind[cur] != 0)
		{
			goto search_to_left;
		}

		curprev = cur;
		cur++;
		for (int i = 0; i < search.Length; i++)
		{
			if (search[i] < edictind[cur + i])
			{
				cur = curprev;
				goto search_to_left;
			}
			if (search[i] > edictind[cur + i]) goto not_found;
		}

	found:
		//MessageBox.Show("Found");
		//search to the left a bit more

		while (true)
		{
			cur--;
			if (cur < 0)
			{
				cur++;
				break;
			}
			while (edictind[cur] != 0) cur--;
			indexLast = cur;
			cur++;
			for (int i = 0; i < search.Length; i++)
			{
				if (search[i] != edictind[cur + i])
				{
					while (edictind[cur] != 0) cur++;
					goto outtahere;
				}
			}
			cur = indexLast;
		}
	outtahere:

		while (edictind[cur] != 0) cur--;
		indexLast = cur;

		if (Compare(search, cur) != 0)
		{
			return false; //TODO: probably never happens
		}

		while (edictind[cur] != 1) cur++;
		cur++;

		while (true)
		{
			int num = 0;
			int rem;
			while (true)
			{
				rem = edictind[cur++];
				if (rem == 0 || rem == 1) break;
				if ((rem >> 2) == 0) rem = edictind[cur++] >> 2;
				num = num * 0x100 + rem;
			}

			searchResults.Add(num);
			longestResult = 1;

			if (rem == 0) break;
		}
		return false;

	not_found:
		//output = "not found";
		return true;

	}

	private int Compare(byte[] search, int cur)
	{
		cur++;
		for (int i = 0; i < search.Length; i++)
		{
			if (search[i] < edictind[cur])
			{
				return -1;
			}
			else if (search[i] > edictind[cur])
			{
				return 1;
			}
			cur++;
		}
		if (edictind[cur] == 1) return 0;

		return 1;
	}

	private void Search(byte[] search)
	{
		if (searchResults.Count > 500) return; //LATER: delete this when you confirm this works

		for (int i = deinflectRuleData.Length - 1; i >= 0; i--)
		{
			if (EndsWith(search, deinflectRuleData[i].from))
			{
				var newsearch = new byte[search.Length - deinflectRuleData[i].from.Length + deinflectRuleData[i].to.Length];
				//search.CopyTo(newsearch, 0);
				Array.Copy(search, newsearch, search.Length - deinflectRuleData[i].from.Length);
				deinflectRuleData[i].to.CopyTo(newsearch, search.Length - deinflectRuleData[i].from.Length);
				//var newsearch_str = eucjp.GetString(newsearch); //LATER: delete
				Search(newsearch);
				//break;
			}
		}

		int cur = indexLast;
		int comp = Compare(search, cur);
		if (comp == 0) goto found;
		if (comp < 0)
		{
			while (true)
			{
				cur--;
				if (cur < 0)
				{
					cur = 0;
					goto not_found;
				}
				while (edictind[cur] != 0) cur--;
				comp = Compare(search, cur);
				if (comp == 0) goto found;
				else if (comp > 0) goto not_found;
			}
		}
		else if (comp > 0)
		{
			while (true)
			{
				cur++;
				if (cur >= edictind.Length)
				{
					cur = edictind.Length - 1;
					goto not_found;
				}
				while (edictind[cur] != 0) cur++;
				comp = Compare(search, cur);
				if (comp == 0) goto found;
				else if (comp < 0) goto not_found;
			}
		}

	found:
		var oldcur = cur;
		while (edictind[cur] != 1) cur++;
		var word_length = (cur - oldcur - 1) / 2;
		cur++;

		while (true)
		{
			int num = 0;
			int rem;
			while (true)
			{
				rem = edictind[cur++];
				if (rem == 0 || rem == 1) break;
				if ((rem >> 2) == 0) rem = edictind[cur++] >> 2;
				num = num * 0x100 + rem;
			}

			bool num_found = false;
			foreach (var s in searchResults)
			{
				if (s == num)
				{
					num_found = true;
					break;
				}
			}
			if (!num_found)
			{
				//var offset = num;
				//int temp_len = 1;
				//while (edict[offset + temp_len] != '\n') temp_len++;
				//var temp = new byte[temp_len];
				//Array.Copy(edict, offset, temp, 0, temp_len);
				//var output = eucjp.GetString(temp);

				//I want longest word to be at the end of the list, it's usually the most important one.
				//Also length of the longest result is used to highlight the word in the window.
				if (longestResult < word_length)
				{
					longestResult = word_length;
					searchResults.Add(num);
				}
				else
				{

					if (searchResults.Count > 0)
					{
						searchResults.Insert(searchResults.Count - 1, num);
					}
					else
					{
						searchResults.Add(num);
					}
				}

			}

			if (rem == 0) break;
		}


	not_found:
		while (edictind[cur] != 0) cur--;
		indexLast = cur;

		return;
	}

	private static bool EndsWith(byte[] s, byte[] send)
	{
		if (s.Length <= send.Length) return false;
		int i = s.Length - send.Length;
		int iend = 0;
		while (true)
		{
			if (iend >= send.Length)
			{
				return true;
			}
			if (s[i] != send[iend])
			{
				return false;
			}
			i++;
			iend++;
		}
	}

	private static int GuessWidth(string s, StringMeasure stringMeasure, int screenWidth)
	{
		//return stringMeasure.GetDisplayLength(s, Config.Font);
		return s.Length * 16;
	}

	public void OnPaint(Graphics graph, StringMeasure stringMeasure, int screenWidth)
	{
		if (!enabled) return;

		if (edict == null)
		{
			Init();
			if (!enabled) return;
			InitData();
			//Autogenerate();
			blueBrush = new SolidBrush(Config.RikaiColorBack);
		}

		if (hidden) return;

		if (SearchFirst())
		{
			return;
		}

		int actualLongestResult = 1;

		for (int ii = 2; ii < 30; ii++)
		{
			int lastlen = searchResults.Count;
			if (ii > output.Length)
			{
				break;
			}
			var search = eucjp.GetBytes(output.Substring(0, ii));
			Search(search);
			if (searchResults.Count != lastlen) actualLongestResult = ii;
		}

		if (searchResults.Count == 0)
		{
			return;
		}

		int x;
		int i = strpos - 1;
		if (i >= css.Ends.Length - 1)
		{
			x = css.PointX + css.Ends[css.Ends.Length - 1];
		}
		else if (i < 0)
		{
			x = css.PointX;
		}
		else
		{
			x = css.PointX + css.Ends[i];
		}
		int y = curLineY;
		int xend;
		i = strpos + actualLongestResult - 1;
		if (i >= css.Ends.Length - 1)
		{
			xend = css.PointX + css.Ends[css.Ends.Length - 1];
		}
		else if (i < 0)
		{
			xend = css.PointX;
		}
		else
		{
			xend = css.PointX + css.Ends[i];
		}

		//Drawing line above selected word
		graph.FillRectangle(blueBrush, x, y - 4, xend - x + 4, 2);

		//Drawing line below selected word
		graph.FillRectangle(blueBrush, x, y + Config.LineHeight + 2, xend - x + 4, 2);

		y -= 30;

		var linesPerBox = new List<int>(16);
		var outputList = new List<string>(16);
		var outputList2 = new List<string>(16);
		var lengths = new List<int>(16);
		int length_max = 0;

		int iend = searchResults.Count - 9;
		if (iend < 0) iend = 0;
		for (i = searchResults.Count - 1; i >= iend; i--)
		{
			var offset = searchResults[i];
			int temp_len = 1;
			while (edict[offset + temp_len] != '\n') temp_len++;
			var temp = new byte[temp_len];
			Array.Copy(edict, offset, temp, 0, temp_len);
			var output = eucjp.GetString(temp);

			int ind;
			while ((ind = output.IndexOf('/', StringComparison.Ordinal)) != -1)
			{
				output = output.Substring(0, ind) + "; " + output.Substring(ind + 1);
			}




			if ((ind = output.IndexOf("(1)", StringComparison.Ordinal)) != -1 && (ind = output.IndexOf("(2)", StringComparison.Ordinal)) != -1)
			{
				int prevSplit = 0;
				int split = ind;
				int linesPerThisBox = 2;
				for (int j = 3; ; j++)
				{
					while (output[split] != ';') split--;
					split++;
					if (j == 3)
					{
						outputList.Add(output.Substring(prevSplit, split - prevSplit));
					}
					else
					{
						outputList.Add("    " + output.Substring(prevSplit, split - prevSplit));
						linesPerThisBox++;
					}
					split++;
					prevSplit = split;
					if ((ind = output.IndexOf("(" + j + ")", StringComparison.Ordinal)) != -1)
					{
						split = ind;
					}
					else
					{
						outputList.Add("    " + output.Substring(prevSplit));
						linesPerBox.Add(linesPerThisBox);
						break;
					}
				}
			}
			else
			{
				outputList.Add(output);
				linesPerBox.Add(1);
			}

			//TextRenderer.DrawText(graph, output, Config.Font, new Point(30, y), Config.FocusColor, TextFormatFlags.NoPrefix);


		}

		int currentBoxIndex = 0;
		int currentBoxLine = 1;

		foreach (var output2 in outputList)
		{
			string s = output2;
			string sprev = s;
			int len;
		do_it_again:
			len = stringMeasure.GetDisplayLength(s, Config.DefaultFont);
			if (len > screenWidth - 32)
			{
				int split;
				while (true)
				{
					split = s.LastIndexOf(' ');
					s = s.Substring(0, split);
					len = stringMeasure.GetDisplayLength(s, Config.DefaultFont);
					if (len > screenWidth - 32)
					{
						continue;
					}
					else
					{
						outputList2.Add(s);
						lengths.Add(len);

						linesPerBox[currentBoxIndex]++;
						currentBoxLine++;

						if (len > length_max) length_max = len;
						s = "      " + sprev.Substring(s.Length + 1);
						sprev = s;
						goto do_it_again;
					}
				}
			}
			else
			{
				outputList2.Add(s);
				lengths.Add(len);
				if (len > length_max) length_max = len;
			};

			currentBoxLine++;
			if (currentBoxIndex < linesPerBox.Count && currentBoxLine > linesPerBox[currentBoxIndex])
			{
				currentBoxLine = 1;
				currentBoxIndex++;
			}
		}

		int x_offset = x - length_max / 2;
		if (x_offset < 16)
		{
			x_offset = 16;
		}
		else if (x_offset + length_max > screenWidth - 16)
		{
			x_offset = screenWidth - 16 - length_max;
		}

		int oldy = y;
		i = 0;
		y += 20;
		if (Config.RikaiUseSeparateBoxes)
		{
			foreach (int linenum in linesPerBox)
			{
				int j = linenum;
				int box_length_max = 0;
				while (j != 0)
				{
					int line_length = lengths[i];
					if (line_length > box_length_max) box_length_max = line_length;
					j--;
					i++;
				}
				y -= 20 * linenum; //LATER: use proper line height
				graph.FillRectangle(blueBrush, x_offset, y, box_length_max, 20 * linenum);
			}
		}
		else
		{
			int linesTotal = 0;
			foreach (int linenum in linesPerBox)
			{
				linesTotal += linenum;
				y -= 20 * linenum;
			}
			graph.FillRectangle(blueBrush, x_offset, y, length_max, 20 * linesTotal);

		}

		y = oldy;

		i = 0;
		foreach (int linenum in linesPerBox)
		{
			int j = linenum;
			int firstlineind = i;
			while (j != 0)
			{
				var line = outputList2[firstlineind + j - 1];
				TextRenderer.DrawText(graph, line, Config.DefaultFont, new Point(x_offset, y), Config.RikaiColorText, TextFormatFlags.NoPrefix);
				j--;
				i++;
				y -= 20;
				if (y < -20) break;
			}
		}
	}
}
