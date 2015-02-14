using System;
using System.Collections;
using System.Collections.Generic;

namespace SharpCircuit {
	
	public class LogicInputElm : SwitchElm {

		//public ElementLead leadOut 	{ get { return lead0; }}

		public double highVoltage 	{ get; set; }
		public double lowVoltage 	{ get; set; }
		public bool isTernary		{ get; set; }
		public bool isNumeric 		{ get; set; }

		public LogicInputElm() : base() {
			highVoltage = 5;
			lowVoltage = 0;
		}

		public override int getLeadCount() {
			return 1;
		}

		public override void setCurrent(int vs, double c) {
			current = -c;
		}

		public override void stamp(CirSim sim) {
			double v = (position == 0) ? lowVoltage : highVoltage;
			if (isTernary)
				v = position * 2.5;
			sim.stampVoltageSource(0, nodes[0], voltSource, v);
		}

		public override int getVoltageSourceCount() {
			return 1;
		}

		public override double getVoltageDiff() {
			return volts[0];
		}

		public override void getInfo(String[] arr) {
			arr[0] = "logic input";
			arr[1] = (position == 0) ? "low" : "high";
			if (isNumeric)
				arr[1] = "" + position;
			arr[1] += " (" + getVoltageText(volts[0]) + ")";
			arr[2] = "I = " + getCurrentText(current);
		}

		public override bool hasGroundConnection(int n1) {
			return true;
		}

	}
}