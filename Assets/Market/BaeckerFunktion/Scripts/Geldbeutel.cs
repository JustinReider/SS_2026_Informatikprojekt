using UnityEngine;

public class Geldbeutel : MonoBehaviour
{
    public int as_Muenzen = 100;

    public bool BezahleAs(int betrag)
    {
        if (as_Muenzen >= betrag)
        {
            as_Muenzen -= betrag;
            Debug.Log("Bezahlt! Noch " + as_Muenzen + " As übrig.");
            return true;
        }
        else
        {
            Debug.Log("Nicht genug As!");
            return false;
        }
    }
}
