﻿using Assets.Scripts.EventSystems;

namespace Assets.Scripts.Collectible
{
    class Medicine : AbstractCollectible
    {
        public override void OnCollected()
        {
            EventSystem.Current.MedicineCollected();
        }

        public override void OnMissed()
        {
            EventSystem.Current.MedicineMissed();
        }
    }
}
