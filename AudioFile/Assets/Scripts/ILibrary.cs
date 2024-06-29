/*Create a ILibrary interface for the MediaLibrary and VisualizerLibrary classes. 
    / This interface will have Add(), Remove(), GetSelection(), and GetSelectionCount() methods.

    / The Add() method will add a media item to the library.
    / The Remove() method will remove a media item from the library.
    / The GetSelection() method will return the selected media item.
    / The GetSelectionCount() method will return the number of selected media items.
    / The ClearSelection() method will clear the selected media items.
    / The GetItems() method will return all the media items in the library.
    / The GetItemCount() method will return the number of media items in the library.
    / The GetSelectedItems() method will return the selected media items in the library.
    / The GetSelectedItemCount() method will return the number of selected media items in the library.
    / The Clear() method will clear all the media items in the library.
    / The SelectAll() method will select all the media items in the library.
    / The DeselectAll() method will deselect all the media items in the library.
    / The GetItem() method will return the media item at the specified index.
    / The GetSelectedItem() method will return the selected media item at the specified index.
    / The SelectItem() method will select the media item at the specified index.
    / The DeselectItem() method will deselect the media item at the specified index.
    / The IsItemSelected() method will return true if the media item at the specified index is selected.
        / The IsItemVisible() method will return true if the media item at the specified index is visible.
    /Create this interface
*/

using System;
using System.Collections;
using System.Collections.Generic;

public interface ILibrary<T> : IList<T>, IComparable<T> where T : IPlayable
{
    //void Sort(SortCriteria criteria); 
    //void Add(MediaItem item); //Add() from IList<T> does the same thing
    //void Remove(MediaItem item); //Remove() from IList<T> does the same thing
        T GetSelection();
    //  int GetSelectionCount(); Count() from IList<T> does the same thing
        void ClearSelection();
        // List<MediaItem> GetItems();
    //    int GetItemCount();
        List<T> GetSelectedItems();
    //    int GetSelectedItemCount();
    //    void Clear();
        void SelectAll();
        void DeselectAll();
    //    MediaItem GetItem(int index); Item[Int32] from IList<T> does the same thing
        T GetSelectedItem(int index);
        void SelectItem(int index);
        void DeselectItem(int index);

    // Checks if the media item at the specified index is selected.
    // Returns true if selected, false otherwise.
        bool IsItemSelected(int index);
     
}







