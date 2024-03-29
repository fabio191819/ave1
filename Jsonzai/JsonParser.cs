﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Jsonzai
{
    public class JsonParser
    {
        private static PropertyInfo[] TargetProperties;
        static readonly Type[] PARSE_ARGUMENTS_TYPES = { typeof(string) };

        public static object Parse(String source, Type klass)
        {
            return Parse(new JsonTokens(source), klass);
        }

        static object Parse(JsonTokens tokens, Type klass) {
            switch (tokens.Current) {
                case JsonTokens.OBJECT_OPEN:
                    return ParseObject(tokens, klass);
                case JsonTokens.ARRAY_OPEN:
                    return ParseArray(tokens, klass);
                case JsonTokens.DOUBLE_QUOTES:
                    return ParseString(tokens);
                default:
                    return ParsePrimitive(tokens, klass);
            }
        }

        private static string ParseString(JsonTokens tokens)
        {
            tokens.Pop(JsonTokens.DOUBLE_QUOTES); // Discard double quotes "
            return tokens.PopWordFinishedWith(JsonTokens.DOUBLE_QUOTES);
        }

        private static object ParsePrimitive(JsonTokens tokens, Type klass)
        {
            string word = tokens.popWordPrimitive();
            if (!klass.IsPrimitive || typeof(string).IsAssignableFrom(klass))
                if (word.ToLower().Equals("null"))
                    return null;
                else
                    throw new InvalidOperationException("Looking for a primitive but requires instance of " + klass);
            /*********** TODO *************/
            // Invoke the corresponding Parse method of klass
            if (klass.Name.Equals("Int32"))
            {
                 return Int32.Parse(word);
              
            }            


            throw new NotImplementedException();
        }

        private static object ParseObject(JsonTokens tokens, Type klass)
        {
            TargetProperties = klass.GetProperties();
            tokens.Pop(JsonTokens.OBJECT_OPEN); // Discard bracket { OBJECT_OPEN
            object target = Activator.CreateInstance(klass);
            return FillObject(tokens, target);
        }

        private static object FillObject(JsonTokens tokens, object target)
        {
            while (tokens.Current != JsonTokens.OBJECT_END)
            {
                String key;
                try
                {
                    key = tokens.PopWordFinishedWith(JsonTokens.COLON);
                }
                catch (Exception e)
                {
                    throw new InvalidOperationException();
                }
                foreach (var m in TargetProperties)
                {
                    if (m.Name.Equals(key))
                    {
                        m.SetValue(target, Parse(tokens, m.PropertyType));
                        break;
                    } 
                }
                if(tokens.Current == JsonTokens.COMMA)
                    {
                        tokens.Pop(JsonTokens.COMMA);
                    }


            }
            tokens.Pop(JsonTokens.OBJECT_END); // Discard bracket } OBJECT_END
            return target;
        }

        private static object ParseArray(JsonTokens tokens, Type klass)
        {
            ArrayList list = new ArrayList();
            tokens.Pop(JsonTokens.ARRAY_OPEN); // Discard square brackets [ ARRAY_OPEN
            while (tokens.Current != JsonTokens.ARRAY_END)
            {
                /*********** TODO *************/
                list.Add(Parse(tokens, klass));

                if (tokens.Current == JsonTokens.COMMA)
                {
                    tokens.Pop(JsonTokens.COMMA);
                    tokens.Trim();
                }
            }
            tokens.Pop(JsonTokens.ARRAY_END); // Discard square bracket ] ARRAY_END
            return list.ToArray(klass);
        }
    }
}
