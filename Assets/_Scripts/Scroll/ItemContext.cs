using System;

namespace StellaCircles.ScrollView
{
    public class ItemContext
    {
        public int SelectedIndex = -1;
        public int SelectedAgeIndex = -1;
        public Action<int> OnCellClicked;
    }
}