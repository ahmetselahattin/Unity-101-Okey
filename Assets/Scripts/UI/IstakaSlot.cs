using UnityEngine;
using UnityEngine.EventSystems;

public class IstakaSlot : MonoBehaviour, IDropHandler
{
    public void OnDrop(PointerEventData eventData)
    {
        if (eventData.pointerDrag != null)
        {
            // Sürüklenen objede senin yazdýðýn DraggableTile var mý kontrol et
            DraggableTile suruklenenTas = eventData.pointerDrag.GetComponent<DraggableTile>();

            if (suruklenenTas != null)
            {
                // DURUM 1: Eðer bu yuva (slot) boþsa, taþý doðrudan buraya al
                // (Placeholder varsa childCount 1 olabilir, o yüzden onu da hesaba katýyoruz)
                if (transform.childCount == 0 || (transform.childCount == 1 && transform.GetChild(0).name == "Placeholder"))
                {
                    suruklenenTas.ParentToReturnTo = this.transform;
                }
                // DURUM 2: Eðer bu yuvada zaten baþka bir taþ varsa, TAÞLARI YER DEÐÝÞTÝR (Swap)
                else
                {
                    Transform mevcutTas = transform.GetChild(0);
                    if (mevcutTas.name == "Placeholder" && transform.childCount > 1)
                    {
                        mevcutTas = transform.GetChild(1); // Placeholder harici gerçek taþý bul
                    }

                    // Eski taþý, sürüklediðimiz taþýn geldiði eski yuvaya gönderiyoruz
                    mevcutTas.SetParent(suruklenenTas.ParentToReturnTo);
                    mevcutTas.localPosition = Vector3.zero; // Tam merkeze oturt

                    // Sürüklediðimiz yeni taþý ise bu yuvaya alýyoruz
                    suruklenenTas.ParentToReturnTo = this.transform;
                }
            }
        }
    }
}