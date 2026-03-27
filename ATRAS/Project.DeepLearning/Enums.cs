using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Project.DeepLearning.UI
{
    public enum DeepLearningSequence
    {
        Unknown,
        
        Idle,

        RecipeLoad,

        RecipeSave,

        SetDataSet,

        Training,

        Predict,

        MultiPredict,
    }
}
