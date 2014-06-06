using System;
using System.Collections;
using System.Collections.Generic;

namespace Circuits {

	public class TriodeElm : CircuitElm {
		public double mu, kg1;
		public double currentp, currentg, currentc;
		public double gridCurrentR = 6000;

		public TriodeElm(int xx, int yy, CirSim s) : base (xx,yy,s) {
			mu = 93;
			kg1 = 680;
			setup();
		}

		void setup() {
			noDiagonal = true;
		}

		public override bool nonLinear() {
			return true;
		}

		public override void reset() {
			volts[0] = volts[1] = volts[2] = 0;
			curcount = 0;
		}

		public Point plate;
		public Point grid; 
		public Point cath; 

//		public override void setPoints() {
//			base.setPoints();
//			plate = newPointArray(4);
//			grid = newPointArray(8);
//			cath = newPointArray(4);
//			grid[0] = point1;
//			int nearw = 8;
//			interpPoint(point1, point2, plate[1], 1, nearw);
//			int farw = 32;
//			interpPoint(point1, point2, plate[0], 1, farw);
//			int platew = 18;
//			interpPoint2(point2, plate[1], plate[2], plate[3], 1, platew);
//
//			circler = 24;
//			interpPoint(point1, point2, grid[1], (dn - circler) / dn, 0);
//			int i;
//			for (i = 0; i != 3; i++) {
//				interpPoint(grid[1], point2, grid[2 + i * 2], (i * 3 + 1) / 4.5, 0);
//				interpPoint(grid[1], point2, grid[3 + i * 2], (i * 3 + 2) / 4.5, 0);
//			}
//			midgrid = point2;
//
//			int cathw = 16;
//			midcath = interpPoint(point1, point2, 1, -nearw);
//			interpPoint2(point2, plate[1], cath[1], cath[2], -1, cathw);
//			interpPoint(point2, plate[1], cath[3], -1.2, -cathw);
//			interpPoint(point2, plate[1], cath[0], -farw / (double) nearw, cathw);
//		}

		/*public override void draw(Graphics g) {
			g.setColor(Color.gray);
			drawThickCircle(g, point2.x, point2.y, circler);
			setBbox(point1, plate[0], 16);
			adjustBbox(cath[0].x, cath[1].y, point2.x + circler, point2.y + circler);
			setPowerColor(g, true);
			// draw plate
			setVoltageColor(g, volts[0]);
			drawThickLine(g, plate[0], plate[1]);
			drawThickLine(g, plate[2], plate[3]);
			// draw grid
			setVoltageColor(g, volts[1]);
			int i;
			for (i = 0; i != 8; i += 2) {
				drawThickLine(g, grid[i], grid[i + 1]);
			}
			// draw cathode
			setVoltageColor(g, volts[2]);
			for (i = 0; i != 3; i++) {
				drawThickLine(g, cath[i], cath[i + 1]);
			}
			// draw dots
			curcountp = updateDotCount(currentp, curcountp);
			curcountc = updateDotCount(currentc, curcountc);
			curcountg = updateDotCount(currentg, curcountg);
			if (sim.dragElm != this) {
				drawDots(g, plate[0], midgrid, curcountp);
				drawDots(g, midgrid, midcath, curcountc);
				drawDots(g, midcath, cath[1], curcountc + 8);
				drawDots(g, cath[1], cath[0], curcountc + 8);
				drawDots(g, point1, midgrid, curcountg);
			}
			drawPosts(g);
		}*/

		public override Point getPost(int n) {
			return (n == 0) ? plate : (n == 1) ? grid : cath;
		}

		public override int getPostCount() {
			return 3;
		}

		public override double getPower() {
			return (volts[0] - volts[2]) * current;
		}

		public double lastv0, lastv1, lastv2;

		public override void doStep() {
			double[] vs = new double[3];
			vs[0] = volts[0];
			vs[1] = volts[1];
			vs[2] = volts[2];
			if (vs[1] > lastv1 + .5) {
				vs[1] = lastv1 + .5;
			}
			if (vs[1] < lastv1 - .5) {
				vs[1] = lastv1 - .5;
			}
			if (vs[2] > lastv2 + .5) {
				vs[2] = lastv2 + .5;
			}
			if (vs[2] < lastv2 - .5) {
				vs[2] = lastv2 - .5;
			}
			int grid = 1;
			int cath = 2;
			int plate = 0;
			double vgk = vs[grid] - vs[cath];
			double vpk = vs[plate] - vs[cath];
			if (Math.Abs(lastv0 - vs[0]) > .01 || Math.Abs(lastv1 - vs[1]) > .01
					|| Math.Abs(lastv2 - vs[2]) > .01) {
				sim.converged = false;
			}
			lastv0 = vs[0];
			lastv1 = vs[1];
			lastv2 = vs[2];
			double ids = 0;
			double gm = 0;
			double Gds = 0;
			double ival = vgk + vpk / mu;
			currentg = 0;
			if (vgk > .01) {
				sim.stampResistor(nodes[grid], nodes[cath], gridCurrentR);
				currentg = vgk / gridCurrentR;
			}
			if (ival < 0) {
				// should be all zero, but that causes a singular matrix,
				// so instead we treat it as a large resistor
				Gds = 1e-8;
				ids = vpk * Gds;
			} else {
				ids = Math.Pow(ival, 1.5) / kg1;
				double q = 1.5 * Math.Sqrt(ival) / kg1;
				// gm = dids/dgk;
				// Gds = dids/dpk;
				Gds = q;
				gm = q / mu;
			}
			currentp = ids;
			currentc = ids + currentg;
			double rs = -ids + Gds * vpk + gm * vgk;
			sim.stampMatrix(nodes[plate], nodes[plate], Gds);
			sim.stampMatrix(nodes[plate], nodes[cath], -Gds - gm);
			sim.stampMatrix(nodes[plate], nodes[grid], gm);

			sim.stampMatrix(nodes[cath], nodes[plate], -Gds);
			sim.stampMatrix(nodes[cath], nodes[cath], Gds + gm);
			sim.stampMatrix(nodes[cath], nodes[grid], -gm);

			sim.stampRightSide(nodes[plate], rs);
			sim.stampRightSide(nodes[cath], -rs);
		}

		public override void stamp() {
			sim.stampNonLinear(nodes[0]);
			sim.stampNonLinear(nodes[1]);
			sim.stampNonLinear(nodes[2]);
		}

		/*public override void getInfo(String arr[]) {
			arr[0] = "triode";
			double vbc = volts[0] - volts[1];
			double vbe = volts[0] - volts[2];
			double vce = volts[1] - volts[2];
			arr[1] = "Vbe = " + getVoltageText(vbe);
			arr[2] = "Vbc = " + getVoltageText(vbc);
			arr[3] = "Vce = " + getVoltageText(vce);
		}

		// grid not connected to other terminals
		public override bool getConnection(int n1, int n2) {
			return !(n1 == 1 || n2 == 1);
		}*/
	}
}