using System.Collections.Generic;
using UnityEngine;

namespace Managers.Entities
{
    [System.Serializable]
    public class UserEntity : MonoBehaviour
    {
    	public float id;
    	public string userName;
    	public int experience;

    	public UserEntity(float id, string userName, int experience)
    	{
    		this.id = id;
    		this.userName = userName;
    		this.experience = experience;
    	}
    }
}