using BepInEx.Configuration;

namespace Vehicle_Map_Marker
{
  public class Configuration
  {
    public ConfigEntry<bool> ShowShips {get; set; }
    public ConfigEntry<bool> ShowCarts {get; set; }

    public Configuration(ConfigFile config)
    {
      ShowShips = config.Bind("Types", "Show ships", true, "Show ships on the map.");
      ShowCarts = config.Bind("Types", "Show carts", true, "Show carts on the map.");
    }

  }

}