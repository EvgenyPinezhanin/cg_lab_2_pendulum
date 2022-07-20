using System;

namespace computer_graphics_lab_2
{
    class Program
    {
        [STAThread]
        private static void Main()
        {
            using (Pendulum.Program program = new Pendulum.Program(1400, 920, "Pendulum(lab_work_2, variant №6)"))
            {
                program.Run();
            }
        }
    }
}
