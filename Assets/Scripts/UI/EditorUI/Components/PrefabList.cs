﻿using Scripts.Building.PrefabsSpawning;

namespace Scripts.UI.EditorUI.Components
{
    public class PrefabList : ListWindowBase<PrefabBase, PrefabListButton>
    {
        protected override string GetItemIdentification(PrefabBase item) => item.gameObject.name;
    }
}