using System;
using System.Collections;
using System.Collections.Generic;

namespace org.zensoftware
{
    /// <summary>
    /// Delegate to observe changes in keys
    /// </summary>
    /// <param name="forKey">The key that was observed.</param>
    /// <param name="forObject">The object the key belongs to.</param>
    /// <param name="oldValue">The key's old value.</param>
    /// <param name="newValue">The key's new value.</param>
    public delegate void observeValueForKey(string forKey, KVCObject forObject, object oldValue, object newValue);

    /// <summary>
    /// An implimentation of Objective-C's Key Value coding.
    /// By Chris Richards 2009.
    /// </summary>
    [Serializable()]
    public class KVCObject
    {
        private Hashtable _properties;
        private Dictionary<string, observeValueForKey> _kvo;

        public KVCObject()
        {
            _properties = new Hashtable();
            _kvo = new Dictionary<string, observeValueForKey>();
        }

        /// <summary>
        /// Get the value for a specified key.
        /// If key doesn't exist, valueForUndefinedKey(string key) will be called instead.
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public object valueForKey(string key)
        {
            if (_properties.ContainsKey(key))
            {
                return _properties[key];
            }
            return valueForUndefinedKey(key);
        }
        /// <summary>
        /// Get the value for a specified key in the keyPath. 
        /// All objects in the path *MUST* inherit from KVCObject for the path to be followed.
        /// </summary>
        /// <param name="keyPath">A Period seprated list of Key values.</param>
        /// <returns></returns>
        public object valueForKeyPath(string keyPath)
        {
            //Get our Key and the rest of the keyPath
            string[] keys = keyPath.Split(".".ToCharArray(), 2);

            //Did we get 2 keys?
            if (keys.Length == 2)
            {
                //Get the Key value, then call it's valueForKeyPath
                object val = valueForKey(keys[0]);
                if (val is KVCObject)    //Verifiy it's the right type
                {
                    return ((KVCObject)val).valueForKeyPath(keys[1]);
                }
                return val;
            }

            //It's just a normal key
            return valueForKey(keys[0]);
        }
        /// <summary>
        /// Called when a key doesn't exist.
        /// Override to impliment your own error handling.
        /// </summary>
        /// <param name="key"></param>
        /// <returns>null</returns>
        public virtual object valueForUndefinedKey(string key)
        {
            return null;
        }
        /// <summary>
        /// Set a value for the key
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        public void setValueForKey(string key, object value)
        {
            //Save the old value
            object oldValue = _properties.ContainsKey(key) ? _properties[key] : null;
            //Change it
            _properties[key] = value;
            //Inform everyone of the change.
            if (_kvo.ContainsKey(key) && _kvo[key] != null) { ((observeValueForKey)_kvo[key])(key, this, oldValue, value); }
        }
        /// <summary>
        /// Set the value for a keyPath.
        /// All objects in the path *MUST* inherit from KVCObject for the path to be followed.
        /// </summary>
        /// <param name="keyPath">A Period seprated list of Key values</param>
        /// <param name="value"></param>
        public void setValueForKeyPath(string keyPath, object value)
        {
            //Get our Key and the rest of the keyPath
            string[] keys = keyPath.Split(".".ToCharArray(), 2);

            //Did we get 2 keys?
            if (keys.Length == 2)
            {
                //Get the Key value, then call it's setValueForKeyPath
                object val = valueForKey(keys[0]);
                if (val is KVCObject)    //Verifiy it's the right type
                {
                    ((KVCObject)val).setValueForKeyPath(keys[1], value);
                }
                else
                {
                    //Not a DBObject, just do a normal set
                    setValueForKey(keys[0], value);
                }
            }
            else
            {
                //It's just a normal key
                setValueForKey(keys[0], value);
            }
        }
        /// <summary>
        /// Add an observer for when the Key changes.
        /// </summary>
        /// <param name="key">The key you want to watch</param>
        /// <param name="observer">delegate observeValueForKey</param>
        public void addObserverForKey(string key, observeValueForKey observer)
        {
            if (!_kvo.ContainsKey(key))
            {
                _kvo.Add(key, observer);
            }
            else
            {
                //Add the observer
                _kvo[key] += observer;
            }
        }
        /// <summary>
        /// Add an observer when the key changes on the key path.
        /// All objects in the path *MUST* inherit from KVCObject for the path to be followed.
        /// </summary>
        /// <param name="keyPath">A Period seprated list of Key values</param>
        /// <param name="observer"></param>
        public void addObserverForKeyPath(string keyPath, observeValueForKey observer)
        {
            //Get our Key and the rest of the keyPath
            string[] keys = keyPath.Split(".".ToCharArray(), 2);

            //Did we get 2 keys?
            if (keys.Length == 2)
            {
                //Get the Key value, then call it's addObserverForKeyPath
                object val = valueForKey(keys[0]);
                if (val is KVCObject)    //Verifiy it's the right type
                {
                    ((KVCObject)val).addObserverForKeyPath(keys[1], observer);
                }
                else
                {
                    //Not a DBObject, just do a normal add
                    addObserverForKey(keys[0], observer);
                }
            }
            else
            {
                //It's just a normal key
                addObserverForKey(keys[0], observer);
            }
        }
        /// <summary>
        /// Remove an observer from the Key changes.
        /// </summary>
        /// <param name="key"></param>
        /// <param name="observer"></param>
        public void removeObserverForKey(string key, observeValueForKey observer)
        {
            if (_kvo.ContainsKey(key))
            {
                _kvo[key] -= observer;
            }
        }
        /// <summary>
        /// Remove an observer from the Key changes.
        /// All objects in the path *MUST* inherit from KVCObject for the path to be followed.
        /// </summary>
        /// <param name="keyPath"></param>
        /// <param name="observer"></param>
        public void removeObserverForKeyPath(string keyPath, observeValueForKey observer)
        {
            //Get our Key and the rest of the keyPath
            string[] keys = keyPath.Split(".".ToCharArray(), 2);

            //Did we get 2 keys?
            if (keys.Length == 2)
            {
                //Get the Key value, then call it's addObserverForKeyPath
                object val = valueForKey(keys[0]);
                if (val is KVCObject)    //Verifiy it's the right type
                {
                    ((KVCObject)val).removeObserverForKeyPath(keys[1], observer);
                }
                else
                {
                    //Not a DBObject, just do a normal add
                    removeObserverForKey(keys[0], observer);
                }
            }
            else
            {
                //It's just a normal key
                removeObserverForKey(keys[0], observer);
            }
        }
        /// <summary>
        /// Returns a list of all the known keys.
        /// </summary>
        /// <returns></returns>
        public List<string> keys()
        {
            List<string> key_list = new List<string>();
            foreach (string key in _properties.Keys)
            {
                key_list.Add(key);
            }
            return key_list;
        }
        /// <summary>
        /// Returns True if there are any Keys defined.
        /// </summary>
        /// <returns></returns>
        public bool hasKeys()
        {
            if (_properties.Count > 0)
            {
                return true;
            }
            return false;
        }
    }
}
