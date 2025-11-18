namespace MoonWorks.Input
{
	/// <summary>
	/// A virtual button corresponding to a direction on a joystick.
	/// If the axis value exceeds the threshold, it will be treated as a press.
	/// </summary>
	public class AxisButton : VirtualButton
	{
		public Axis Parent { get; }
		public AxisButtonCode Code { get; }

		private float threshold = 0.5f;
		public float Threshold
		{
			get => threshold;
			set => threshold = System.Math.Clamp(value, 0, 1);
		}

		private int Sign;

		internal AxisButton(Axis parent, bool positive)
		{
			Parent = parent;
			Sign = positive ? 1 : -1;

			if (parent.Code == AxisCode.LeftX)
			{
				if (positive)
				{
					Code = AxisButtonCode.LeftX_Right;
				}
				else
				{
					Code = AxisButtonCode.LeftX_Left;
				}
			}
			else if (parent.Code == AxisCode.LeftY)
			{
				if (positive)
				{
					Code = AxisButtonCode.LeftY_Down;
				}
				else
				{
					Code = AxisButtonCode.LeftY_Up;
				}
			}
			else if (parent.Code == AxisCode.RightX)
			{
				if (positive)
				{
					Code = AxisButtonCode.RightX_Right;
				}
				else
				{
					Code = AxisButtonCode.RightX_Left;
				}
			}
			else if (parent.Code == AxisCode.RightY)
			{
				if (positive)
				{
					Code = AxisButtonCode.RightY_Down;
				}
				else
				{
					Code = AxisButtonCode.RightY_Up;
				}
			}
		}

		internal void Update()
        {
            UpdateState(Sign * Parent.Value >= threshold);
        }
	}
}
