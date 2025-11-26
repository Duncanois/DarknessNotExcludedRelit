namespace DarknessNotIncluded
{
  public class UnitOrientation : KMonoBehaviour, ISim33ms
  {
    public enum Orientation
    {
      Unknown,
      Left,
      UpLeft,
      Up,
      UpRight,
      Right,
      DownRight,
      Down,
      DownLeft,
    }

    [MyCmpGet]
    private Navigator navigator;

    public Orientation orientation = Orientation.Unknown;

    public void Sim33ms(float dt)
    {
      var newOrientation = GetCurrentOrientation();
      if (newOrientation != Orientation.Unknown)
      {
        orientation = newOrientation;
      }
    }

    private Orientation GetCurrentOrientation()
    {
      if (navigator == null || !navigator.IsMoving() || !navigator.path.IsValid())
      {
        // Use transform scale as fallback for facing direction
        var scaleX = gameObject.transform.localScale.x;
        return scaleX < 0 ? Orientation.Left : Orientation.Right;
      }

      var currCell = Grid.PosToCell(gameObject);

      // Ensure path has at least two nodes
      if (navigator.path.nodes == null || navigator.path.nodes.Count < 2)
        return Orientation.Unknown;

      var nextCell = navigator.path.nodes[1].cell;
      var vert = Grid.CellRow(nextCell) - Grid.CellRow(currCell); // up > 0 > down
      var horiz = Grid.CellColumn(nextCell) - Grid.CellColumn(currCell); // right > 0 > left

      if (horiz < 0 && vert == 0) return Orientation.Left;
      if (horiz < 0 && vert > 0) return Orientation.UpLeft;
      if (horiz == 0 && vert > 0) return Orientation.Up;
      if (horiz > 0 && vert > 0) return Orientation.UpRight;
      if (horiz > 0 && vert == 0) return Orientation.Right;
      if (horiz > 0 && vert < 0) return Orientation.DownRight;
      if (horiz == 0 && vert < 0) return Orientation.Down;
      if (horiz < 0 && vert < 0) return Orientation.DownLeft;

      return Orientation.Unknown;
    }
  }
}
