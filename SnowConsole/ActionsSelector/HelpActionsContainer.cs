using System;
using System.Collections.Generic;
using System.Text;

namespace Cleancode.Snow.ActionsSelector
{
    [ActionsContainer()]
    class HelpActionsContainer
    {
        ActionsTreeNode _actionsRoot;

        public HelpActionsContainer(ActionsTreeNode actionsRoot)
        {
            this._actionsRoot = actionsRoot;
        }

        //[Action("help")]
        //public string GetHelp()
        //{
        // //   Stack<string> build
        //}

        //[Action("help")]
        //public string GetHelp(string command)
        //{

        //}
    }
}