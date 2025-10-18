public interface ISingleSelectionMenu<EntryType> where EntryType : class
{
	EntryType GetSelection();

	bool SetSelection(EntryType selection);
}
