// Game4Automation (R) Framework for Automation Concept Design, Virtual Commissioning and 3D-HMI
// (c) 2019 in2Sight GmbH - Usage of this source code only allowed based on License conditions see https://game4automation.com/lizenz  

//! Interface for all Classes which are Fixing components (currently Grip and Fixer)
namespace game4automation
{
    public interface IFix
    {
        void Fix(MU mu);
        void Unfix(MU mu);
    }
}