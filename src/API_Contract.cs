using System;
using System.Text;
using System.Text.RegularExpressions;
using System.Collections.Generic;

namespace Tabellarius
{

	enum Contract_ProgrammTermin_Typ
	{
		MA = 0,
		PROGRAMM = 1,
		FREIWILLIG = 2,
		AUFBAU = 3,
		ESSEN = 4,
	};

	enum Contract_ProgrammDescr_Visiblity
	{
		NOTE = 0,
		MA_ONLY = 1,
		ALL = 2
	};

	enum Contract_TextParent_Typ
	{
		TITLE = 0,
	};

	enum Contract_TextChild_Typ
	{
		TEXT = 0,
		SUB_HEADING = 1,
	};


	public enum ListColumnID
	{
		Uhrzeit, Text, Typ,
	}

	public enum TextColumnID
	{
		Rang, Typ, Text,
	}

	public class API_Contract
	{

		public static readonly string[] ProgrammTerminTypVal =
		{
			"Nur MAs",
			"Programm",
			"Freiwillig",
			"Aufbau",
			"Essen",
		};

		public static readonly Dictionary<int, string> ProgrammTerminTypHR = new Dictionary<int, string>()
		{
			{ (int)Contract_ProgrammTermin_Typ.MA, ProgrammTerminTypVal[0] },
			{ (int)Contract_ProgrammTermin_Typ.PROGRAMM, ProgrammTerminTypVal[1] },
			{ (int)Contract_ProgrammTermin_Typ.FREIWILLIG, ProgrammTerminTypVal[2] },
			{ (int)Contract_ProgrammTermin_Typ.AUFBAU, ProgrammTerminTypVal[3] },
			{ (int)Contract_ProgrammTermin_Typ.ESSEN, ProgrammTerminTypVal[4] },
			{ -1, ""}
		};

		public static readonly Dictionary<string, int> ProgrammTerminTypCR = new Dictionary<string, int>()
		{
			{ ProgrammTerminTypVal[0], (int)Contract_ProgrammTermin_Typ.MA  },
			{ ProgrammTerminTypVal[1], (int)Contract_ProgrammTermin_Typ.PROGRAMM },
			{ ProgrammTerminTypVal[2], (int)Contract_ProgrammTermin_Typ.FREIWILLIG },
			{ ProgrammTerminTypVal[3], (int)Contract_ProgrammTermin_Typ.AUFBAU },
			{ ProgrammTerminTypVal[4], (int)Contract_ProgrammTermin_Typ.ESSEN },
			{ "", -1}
		};


		public static readonly string[] ProgrammDescrTypVal =
		{
			"Notiz",
		 	"Nur MAs",
		 	"Alle"
		};

		public static readonly Dictionary<int, string> ProgrammDescrTypHR = new Dictionary<int, string>()
		{
			{ (int)Contract_ProgrammDescr_Visiblity.NOTE, ProgrammDescrTypVal[0] },
			{ (int)Contract_ProgrammDescr_Visiblity.MA_ONLY, ProgrammDescrTypVal[1] },
			{ (int)Contract_ProgrammDescr_Visiblity.ALL, ProgrammDescrTypVal[2] },
			{ -1, ""}
		};

		public static readonly Dictionary<string, int> ProgrammDescrTypCR = new Dictionary<string, int>()
		{
			{ ProgrammDescrTypVal[0], (int)Contract_ProgrammDescr_Visiblity.NOTE },
			{ ProgrammDescrTypVal[1], (int)Contract_ProgrammDescr_Visiblity.MA_ONLY },
			{ ProgrammDescrTypVal[2], (int)Contract_ProgrammDescr_Visiblity.ALL },
			{ "", -1 }
		};


		public static readonly string[] CategorieTextTypParentVal =
		{
			"Titel",
		};

		public static readonly Dictionary<int, string> CategorieTextParentTypHR = new Dictionary<int, string>()
		{
			{ (int)Contract_TextParent_Typ.TITLE, CategorieTextTypParentVal[0] },
			{ -1, "" }
		};

		public static readonly Dictionary<string, int> CategorieTextParentTypCR = new Dictionary<string, int>()
		{
			{ CategorieTextTypParentVal[0], (int)Contract_TextParent_Typ.TITLE },
			{ "", -1 }
		};

		public static readonly string[] CategorieTextTypChildVal =
		{
			"Text",
			"Unterüberschrift",
		};

		public static readonly Dictionary<int, string> CategorieTextChildTypHR = new Dictionary<int, string>()
		{
			{ (int)Contract_TextChild_Typ.TEXT, CategorieTextTypChildVal[0] },
			{ (int)Contract_TextChild_Typ.SUB_HEADING, CategorieTextTypChildVal[1] },
			{ -1, "" }
		};

		public static readonly Dictionary<string, int> CategorieTextChildTypCR = new Dictionary<string, int>()
		{
			{ CategorieTextTypChildVal[0], (int)Contract_TextChild_Typ.TEXT },
			{ CategorieTextTypChildVal[1], (int)Contract_TextChild_Typ.SUB_HEADING },
			{ "", -1 }
		};



		public static string ConvertTreeViewToEditView(string s)
		{
			return s.Substring(1).Replace("\\n", "\n");
		}

		public static string ConvertEditViewToDatabase(string s)
		{
			return s.Replace("\n", "\\n");
		}

		public static string ConvertDatabaseToTreeChild(string s)
		{
			return "\t" + s;
		}

		public static void ClearTimeConflicts(Gtk.TreeStore treeContent, Gtk.TreeIter toCheck)
		{
			// Get first Iter with equal time
			var checkVal = (string)treeContent.GetValue(toCheck, (int)ListColumnID.Uhrzeit);
			Gtk.TreeIter before = toCheck;
			Gtk.TreeIter drag = before;
			while (treeContent.IterPrevious(ref before)) {
				string refVal = (string)treeContent.GetValue(before, (int)ListColumnID.Uhrzeit);
				if (refVal.StartsWith(checkVal.Substring(0, 5)))
					drag = before;
				else
					break;
			}

			// Set all values
			bool hasNext = true;
			for (int i = 0; hasNext; i++) {
				SetRang(treeContent, drag, i);
				GtkHelper.SortInByColumn(treeContent, (int)ListColumnID.Uhrzeit, drag);
				hasNext = treeContent.IterNext(ref drag);
				string refVal = (string)treeContent.GetValue(drag, (int)ListColumnID.Uhrzeit);
				if (!refVal.StartsWith(checkVal.Substring(0, 5)))
					break;
			}
		}

		private static void SetRang(Gtk.TreeStore store, Gtk.TreeIter iter, int rang)
		{
			var val = (string)store.GetValue(iter, (int)ListColumnID.Uhrzeit);
			store.SetValue(iter, (int)ListColumnID.Uhrzeit, val.Substring(0, 5) + ":" + rang);
		}

		public static Gtk.TextTag boldTag, italicTag;
		static API_Contract()
		{
			boldTag = new Gtk.TextTag("bold");
			italicTag = new Gtk.TextTag("italic");
			boldTag.Weight = Pango.Weight.Bold;
			italicTag.Style = Pango.Style.Italic;
		}

		public static void ConvertDatabaseToEditCategorie(string raw, ref Gtk.TextBuffer buffer)
		{
			var splittedTags = Regex.Split(raw, "(<.>|</.>|<..>)"); // What a noobish Regex
			var tagList = new List<Gtk.TextTag>();

			// Split text into HTML-tags and insert the text with them
			var endIter = buffer.EndIter;
			foreach (var textBit in splittedTags) {
				switch (textBit) {
					case "<br>":
						buffer.Insert(ref endIter, "\n");
						break;
					case "<b>":
						tagList.Add(boldTag);
						break;
					case "</b>":
						tagList.Remove(boldTag);
						break;
					case "<i>":
						tagList.Add(italicTag);
						break;
					case "</i>":
						tagList.Remove(italicTag);
						break;
					default:
						if (tagList.Count != 0)
							buffer.InsertWithTags(ref endIter, textBit, tagList.ToArray());
						else
							buffer.Insert(ref endIter, textBit);
						break;
				}
			}
		}

		public static string ConvertEditCategorieToDatabse(Gtk.TextBuffer buffer)
		{

			StringBuilder dbBuilder = new StringBuilder(buffer.Text);

			// Replace tags of the textBuffer and save them as HTML-Tags
			int buildOffset = 0;
			for (Gtk.TextIter iter = buffer.StartIter; !iter.IsEnd; iter.ForwardChar()) {

				if (iter.BeginsTag(boldTag)) {
					dbBuilder.Insert(iter.Offset + buildOffset, "<b>", 1);
					buildOffset += 3;
				} else
				if (iter.BeginsTag(italicTag)) {
					dbBuilder.Insert(iter.Offset + buildOffset, "<i>", 1);
					buildOffset += 3;
				}
				if (iter.EndsTag(boldTag)) {
					dbBuilder.Insert(iter.Offset + buildOffset, "</b>", 1);
					buildOffset += 4;
				}
				if (iter.EndsTag(italicTag)) {
					dbBuilder.Insert(iter.Offset + buildOffset, "</i>", 1);
					buildOffset += 4;
				}
			}
			dbBuilder.Replace("\n", "<br>");

			Console.WriteLine(dbBuilder);

			return dbBuilder.ToString();
		}

	}
}