using Gtk;
using System;
using System.Collections.Generic;

namespace Tabellarius.Assets
{
	public class TagTextView : TextView
	{
		private readonly ToggleButton boldButton, italicButton;
		private readonly HashSet<TextTag> activeTags = new HashSet<TextTag>();
		private TextIter applyIter = TextIter.Zero;
		private TextTag BoldTag { get { return API_Contract.boldTag; } }
		private TextTag ItalicTag { get { return API_Contract.italicTag; } }


		public TagTextView(ref ToggleButton boldButton, ref ToggleButton italicButton) : base(new TextBuffer(API_Contract.TagTable))
		{
			this.boldButton = boldButton;
			this.italicButton = italicButton;

			boldButton.Clicked += TagButtonClicked; // TODO: Shortcut
			italicButton.Clicked += TagButtonClicked; // TODO: Shortcut

			this.KeyReleaseEvent += AdjustToggleState; // (Arrow) Keys
			this.ButtonReleaseEvent += AdjustToggleState; // Clicked

			Buffer.InsertText += InsertTextHandler;
		}

		private void TagButtonClicked(object sender, EventArgs args)
		{
			if (this.IsFocus)
				return;

			this.IsFocus = true; // Get cursor back!

			ToggleButton caller = (ToggleButton)sender;
			TextTag checkTag = (caller == boldButton)
								? BoldTag : ItalicTag;

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
						activeTags.Add(tag);

					if (caller.Active) {
						activeTags.Add(checkTag);
					} else {
						activeTags.Remove(checkTag);
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

			if (currIter.Equals(applyIter)) { // Bufferposition didn't change
											  // To applied tags are given by user
				boldButton.Active = activeTags.Contains(BoldTag);
				italicButton.Active = activeTags.Contains(ItalicTag);
			} else {
				// To applied tags are given by text
				activeTags.Clear();
				applyIter = TextIter.Zero;
				boldButton.Active = currIter.HasTag(BoldTag);
				italicButton.Active = currIter.HasTag(ItalicTag);
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
				foreach (var tag in activeTags)
					Buffer.ApplyTag(tag, startIt, endIt);
				applyIter = endIt; // Add CursorPosition
			}
		}

		public void Clear()
		{
			activeTags.Clear();
			applyIter = TextIter.Zero;
			boldButton.Active = italicButton.Active = false;
		}

	}
}