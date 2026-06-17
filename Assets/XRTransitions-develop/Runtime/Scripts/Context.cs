using System.Linq;
using UnityEngine;
using UnityEngine.Events;

namespace Scripts
{
    public class Context : MonoBehaviour
    {
        [field: SerializeField]
        public bool IsAR { private set; get; }
        
        public static UnityAction<Context> OnEnter;
        public static UnityAction<Context> OnExit;
    }
}