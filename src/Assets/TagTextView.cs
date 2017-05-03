using Gtk;
using System;
using System.Collections.Generic;

namespace Tabellarius.Assets
{
	public class TagTextView : TextView
	{

		private readonly ToggleButton boldButton, italicButton;
		private readonly HashSet<TextTag> applyTags;
		private TextIter applyIter;

		public TagTextView(ToggleButton boldButton, ToggleButton italicButton) : base()
		{
			applyTags = new HashSet<TextTag>();
			applyIter = TextIter.Zero;

			this.boldButton = boldButton;
			this.italicButton = italicButton;

			boldButton.Clicked += TagButtonCLicked; // TODO: Shortcut
			italicButton.Clicked += TagButtonCLicked; // TODO: Shortcut

			this.KeyReleaseEvent += AdjustToggleState; // (Arrow) Keys
			this.ButtonReleaseEvent += AdjustToggleState; // Clicked

			Buffer.InsertText += InsertTextHandler;
		}

		private void TagButtonCLicked(object sender, EventArgs args)
		{
			if (this.IsFocus) // Called by AdjustToggleState()
				return; // Nothing to do then

			this.IsFocus = true; // We still need a Cursor

			ToggleButton caller = (ToggleButton)sender;
			TextTag checkTag = (caller == boldButton)
								? API_Contract.boldTag : API_Contract.italicTag;

			TextIter startIt, endIt;
			// Is text selected?
			if (!Buffer.GetSelectionBounds(out startIt, out endIt)) {
				// Check if Cursor is inside a word
				var currIt = Buffer.GetIterAtOffset(Buffer.CursorPosition);

				if (currIt.InsideWord() && !currIt.StartsWord()) {
					// Apply tag to whole word
					startIt = currIt;
					while (!startIt.StartsWord() && !startIt.IsStart)
						startIt.BackwardChar();
					endIt = currIt;
					while (!endIt.EndsWord() && !endIt.IsEnd)
						endIt.ForwardChar();

				} else { // Somehwere between words
						 // Apply tag when user writes on this offset
					applyIter = currIt;

					foreach (var tag in currIt.Tags)
						applyTags.Add(tag);

					if (caller.Active) {
						applyTags.Add(checkTag);
					} else {
						applyTags.Remove(checkTag);
					}
					return; // User has to give new input
				}
			}

			if (caller.Active) {
				Buffer.ApplyTag(checkTag, startIt, endIt);
			} else {
				Buffer.RemoveTag(checkTag, startIt, endIt);
			}
		}

		private void AdjustToggleState(object sender, EventArgs args)
		{
			// CursorPosition got changed and buttons states need adjustment
			TextIter currIter = Buffer.GetIterAtOffset(Buffer.CursorPosition);

			if (currIter.Equals(applyIter)) {
				// To applied tags are given by user
				boldButton.Active = applyTags.Contains(API_Contract.boldTag);
				italicButton.Active = applyTags.Contains(API_Contract.italicTag);
			} else {
				// To applied tags are given by text
				applyTags.Clear();
				applyIter = TextIter.Zero;
				boldButton.Active = currIter.HasTag(API_Contract.boldTag);
				italicButton.Active = currIter.HasTag(API_Contract.italicTag);
			}
		}

		private void InsertTextHandler(object sender, InsertTextArgs args)
		{
			var startIt = args.Pos;
			if (startIt.BackwardChars(args.NewTextLength) // Can go to start
					&& applyIter.Equals(startIt)) {
				// User wants special applied tags here
				var endIt = args.Pos;
				Buffer.RemoveAllTags(startIt, endIt);
				foreach (var tag in applyTags)
					Buffer.ApplyTag(tag, startIt, endIt);
				applyIter = endIt; // Add CursorPosition
			}
		}

		public void Clear() {
			applyTags.Clear();
			applyIter = TextIter.Zero;
			boldButton.Active = italicButton.Active = false;
		}

	}
}