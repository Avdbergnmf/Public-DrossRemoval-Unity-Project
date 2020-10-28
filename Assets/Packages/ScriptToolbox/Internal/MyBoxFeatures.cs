#if UNITY_EDITOR
using UnityEditor;

namespace MyBox.Internal
{
	[InitializeOnLoad]
	public class MyBoxFeatures
	{
		private const string AutoSaveMenuItemKey = "Tools/MyBox/AutoSave on play";

		static MyBoxFeatures()
		{
			AutoSaveIsEnabled = AutoSaveIsEnabled;
		}


		#region AutoSave

		private static bool AutoSaveIsEnabled
		{
			get { return MyBoxSettings.AutoSaveEnabled; }
			set
			{
				{
					MyBoxSettings.AutoSaveEnabled = value;
					AutoSaveFeature.IsEnabled = value;
				}
			}
		}

		[MenuItem(AutoSaveMenuItemKey, priority = 100)]
		private static void AutoSaveMenuItem()
		{
			AutoSaveIsEnabled = !AutoSaveIsEnabled;
		}

		[MenuItem(AutoSaveMenuItemKey, true)]
		private static bool AutoSaveMenuItemValidation()
		{
			Menu.SetChecked(AutoSaveMenuItemKey, AutoSaveIsEnabled);
			return true;
		}

		#endregion
		
	}
}
#endif