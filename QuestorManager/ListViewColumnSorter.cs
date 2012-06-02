using System;
using System.Collections;
using System.Windows.Forms;

public class ListViewColumnSort : IComparer
{
    public enum TipoCompare
    {
        Cadena,
        Numero,
        Fecha
    }

    public TipoCompare CompararPor;
    public int ColumnIndex;
    public SortOrder Sorting = SortOrder.Ascending;

    public ListViewColumnSort()
    {
    }

    public ListViewColumnSort(int columna)
    {
        ColumnIndex = columna;
    }

    public int Compare(Object a, Object b)
    {
        int menor = -1, mayor = 1;
        //
        if (Sorting == SortOrder.None)
            return 0;

        string s1 = ((ListViewItem)a).SubItems[ColumnIndex].Text;
        string s2 = ((ListViewItem)b).SubItems[ColumnIndex].Text;

        if (Sorting == SortOrder.Descending)
        {
            menor = 1;
            mayor = -1;
        }
        //
        switch (CompararPor)
        {
            case TipoCompare.Fecha:
                try
                {
                    DateTime f1 = DateTime.Parse(s1);
                    DateTime f2 = DateTime.Parse(s2);
                    //
                    if (f1 < f2)
                        return menor;
                    else if (f1 == f2)
                        return 0;
                    else
                        return mayor;
                }
                catch
                {
                    return System.String.Compare(s1, s2, System.StringComparison.OrdinalIgnoreCase) * mayor;
                }

            case TipoCompare.Numero:
                try
                {
                    decimal n1 = decimal.Parse(s1);
                    decimal n2 = decimal.Parse(s2);
                    if (n1 < n2)
                        return menor;
                    else if (n1 == n2)
                        return 0;
                    else
                        return mayor;
                }
                catch
                {
                    return System.String.Compare(s1, s2, System.StringComparison.OrdinalIgnoreCase) * mayor;
                }

            default:

                return System.String.Compare(s1, s2, System.StringComparison.OrdinalIgnoreCase) * mayor;
        }
    }
}