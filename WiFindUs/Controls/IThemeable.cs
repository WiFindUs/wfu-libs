
namespace WiFindUs.Controls
{
	public interface IThemeable
	{
		Theme Theme { get; set; }
		void OnThemeChanged();
	}
}
