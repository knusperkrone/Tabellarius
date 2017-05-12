using System;
using Gtk;

namespace Tabellarius
{
	public class GtkHelper
	{

		public static TreeIter SortInByColumn(TreeStore treeContent, int column, TreeIter insertIter)
		{
			// Iter-Interface is limited to linear Search
			var insertString = (string)treeContent.GetValue(insertIter, column);

			TreeIter currIter, swapIter;
			bool insertBefore = false;
			bool insertAfter = false;
			bool hasNext;

			// Walking up from insertIter and search for new position (Use-case)
			currIter = insertIter;
			swapIter = currIter;
			hasNext = treeContent.IterPrevious(ref currIter);
			while (hasNext && !currIter.Equals(insertIter)
					&& String.Compare(insertString, (string)treeContent.GetValue(currIter, column)) < 0) {

				insertBefore = true;
				swapIter = currIter;
				hasNext = treeContent.IterPrevious(ref currIter);
			}

			// Walking down from insertIter
			if (!insertBefore) {
				// Not already found position
				currIter = insertIter;
				swapIter = currIter;
				hasNext = treeContent.IterNext(ref currIter);
				while (hasNext && !currIter.Equals(insertIter)
						&& String.Compare(insertString, (string)treeContent.GetValue(currIter, column)) > 0) {

					insertAfter = true;
					swapIter = currIter;
					hasNext = treeContent.IterNext(ref currIter);
				}
			}


			if (insertBefore) {
				treeContent.MoveBefore(insertIter, swapIter);
			} else if (insertAfter) {
				treeContent.MoveAfter(insertIter, swapIter);
			}
			return insertIter;
		}

		public static string ComboBoxActiveString(ComboBox comboBox) {
			TreeIter tree;
			comboBox.GetActiveIter(out tree);
			return (String)comboBox.Model.GetValue(tree, 0);
		}

		public static void FillComboBox(ComboBox cb, String[] values)
		{
			cb.Clear();

			var cell = new CellRendererText();
			cb.PackStart(cell, false);
			cb.AddAttribute(cell, "text", 0);
			var store = new ListStore(typeof(string));
			foreach (var val in values) {
				store.AppendValues(val);
			}
			cb.Model = store;
		}

		public static TreeIter GetLastIter(TreeStore treeContent, TreeIter iter) {
			TreeIter drag, curr;
			curr = iter;
			drag = iter;
			while (treeContent.IterNext(ref curr)) {
					drag = curr;
			}
			return drag;
		}

		public static int FindInArray(string[] arr, string val)
		{
			// Assert that the @val hast to be in @arr
			for (int i = 0; i < arr.Length; i++) {
				if (arr[i].Equals(val))
					return i;
			}
			return -1;
		}

	}
}