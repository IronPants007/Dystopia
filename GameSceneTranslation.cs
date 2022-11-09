using System.Collections.Generic;
using UnityEngine;
/*
#scene: main 
#background: outside1 
#characters: bob, jane
jane: "   !"
bob: "Hi there! How's your day going?"
+ "Good :)"
    bob: "Yayy!"
    -> END
+ { .happiness < 0 } Bad!!!
    bob: "Oh noooo :("
    { .supplies += 5 }
    bob: "Here's some supplies :)"
    -> END
*/

/*
Setting("scene") Colon Identifier("main") EndStatement
Setting("background") Colon Identifier("outside1") EndStatement
Setting("characters") Colon Identifier("bob") Comma Identifier("jane") EndStatement
Identifier("jane") Colon String("   !") EndStatement
Identifier("bob") Colon String("Hi there! How's your day going?") EndStatement
ChoiceDef String("Good :)") EndStatement
IndentChange(1)
Identifier("bob") Colon String("Yayy!") EndStatement
Arrow Identifier("END") EndStatement
IndentChange(0)
ChoiceDef Code(".happiness < 0") String("Bad!!!") EndStatement
IndentChange(1)
Identifier("bob") Colon String("Oh noooo :(") EndStatement
Code(".supplies += 5") EndStatement
Identifier("bob") Colon String("Here's some supplies :)") EndStatement
Arrow Identifier("END") EndStatement
*/


public class GameSceneTranslater {
    // Script -> Token List
    public static List<Token> Tokenize(string input) {
        List<Token> tokenList = new List<Token>();
        int lineNumber = 1;
        int lineCharIndex = 0;
        int i = 0;
        int lastIndentLevel = 0;
        int indentLevel = 0;
        bool isBeginning = true;
        while (i < input.Length) {
            int lineIndex = i-lineCharIndex;
            char c = input[i];
            if (isBeginning) {
                if (c == ' ') {
                    indentLevel++;
                    i++;
                    continue;
                } else {
                    isBeginning = false;
                    if (indentLevel > lastIndentLevel) {
                        tokenList.Add(new IndentIncToken(lineNumber, lineIndex));
                    } else if (indentLevel < lastIndentLevel) {
                        tokenList.Add(new IndentDecToken(lineNumber, lineIndex));
                    }
                    lastIndentLevel = indentLevel;
                    indentLevel = 0;
                }
            }
            if (IsLetter(c)) { // letter = begin identifier
                string ident = "";
                while (i < input.Length && (IsLetter(input[i]) || input[i] == '_' || IsNum(input[i]))) {
                    ident += input[i];
                    i++;
                }
                if (ident.Length==0) throw new CompileException("Identifier name is empty/invalid." + "\n At " + lineNumber + ":" + lineIndex);
                tokenList.Add(new IdentifierToken(ident, lineNumber, lineIndex));
                continue;
            } else if (c == '#') { // # = begin setting
                string ident = "";
                i++;
                while (i < input.Length && IsLetter(input[i])) {
                    ident += input[i];
                    i++;
                }
                if (ident.Length==0) throw new CompileException("Setting name is empty/invalid." + "\n At " + lineNumber + ":" + lineIndex);
                tokenList.Add(new SettingToken(ident, lineNumber, lineIndex));
                continue;
            } else if (c == '{') { // # = begin code block
                string code = "";
                List<Token> codeTokens = new List<Token>();
                i++;
                while (i < input.Length && input[i]!='}') {
                    code += input[i];
                    i++;
                }
                if (code.Length==0) throw new CompileException("Code is empty." + "\n At " + lineNumber + ":" + lineIndex);
                for (int n = 0; n < code.Length; n++) {
                    if (IsLetter(code[n])) { // letter = begin identifier
                        string ident = "";
                        while (n < code.Length && (IsLetter(code[n]) || input[i] == '_' || IsNum(code[n]))) {
                            ident += code[n];
                            n++;
                        }
                        if (ident.Length==0) throw new CompileException("Identifier name is empty/invalid." + "\n At " + lineNumber + ":" + lineIndex);
                        codeTokens.Add(new IdentifierToken(ident, lineNumber, lineIndex));
                    } else if (IsNum(code[n]) || (code[n] == '-' && IsNum(code[n+1]))) { // letter = begin identifier
                        var mult = 1;
                        if (code[n] == '-') {
                            n++;
                            mult = -1;
                        }
                        string num = "";
                        while (n < code.Length && IsNum(code[n])) {
                            num += code[n];
                            n++;
                        }
                        int resultNum;
                        try {
                            resultNum = int.Parse(num);
                        } catch { throw new CompileException("Identifier name is empty/invalid." + "\n At " + lineNumber + ":" + lineIndex); }
                        resultNum *= mult;
                        codeTokens.Add(new IntToken(resultNum, lineNumber, lineIndex));
                    } else if (code[n] == '>' || code[n] == '<') {
                        if (code[n+1] == '=') {
                            codeTokens.Add(new IdentifierToken(code[n] + "=", lineNumber, lineIndex));
                            n++;
                        } else {
                            codeTokens.Add(new IdentifierToken(code[n] + "", lineNumber, lineIndex));
                        }
                    } else if (code[n] == '=') {
                        codeTokens.Add(new IdentifierToken("=", lineNumber, lineIndex));
                    } else if (code[n] == '!') {
                        if (code[n+1] == '=') {
                            codeTokens.Add(new IdentifierToken(code[n] + "=", lineNumber, lineIndex));
                            n++;
                        } else {
                            throw new CompileException("Expected '=' to complete not-equal condition." + "\n At " + lineNumber + ":" + lineIndex);
                        }
                    } else if (code[n] == '+' || code[n] == '-' || code[n] == '*' || code[n] == '/') {
                        codeTokens.Add(new IdentifierToken(code[n] + "", lineNumber, lineIndex));
                    }
                }
                tokenList.Add(new CodeToken(codeTokens, lineNumber, lineIndex));
            } else if (c == '"' || c == '\'' || c == '`') { // " ' ` = begin/end string
                char f = c;
                string str = "";
                i++;
                while (i < input.Length && input[i]!=f) {
                    if (input[i] == '\n') throw new CompileException("Malformed string. Cannot have newlines." + "\n At " + lineNumber + ":" + lineIndex);
                    str += input[i];
                    i++;
                }
                tokenList.Add(new StringToken(str, lineNumber, lineIndex));
            } else if (c == ':') {
                tokenList.Add(new ColonToken(lineNumber, lineIndex));
            } else if (c == ',') {
                tokenList.Add(new CommaToken(lineNumber, lineIndex));
            } else if (c == '+') {
                tokenList.Add(new ChoiceDefToken(lineNumber, lineIndex));
            } else if (c == '>') {
                tokenList.Add(new ArrowToken(lineNumber, lineIndex));
            } else if (c == '\n') {
                tokenList.Add(new EndToken(lineNumber, lineIndex));
                lineNumber++;
                lineCharIndex = i;
                isBeginning = true;
            } else if (c != ' ' && c!='\r') {
                throw new CompileException("Unrecognized token: " + c + "\n At " + lineNumber + ":" + lineIndex);
            }
            i++;
        }
        return tokenList;
    }


    static bool IsNum(char c) {
        return (c >= '0' && c<= '9');
    }
    static bool IsLetter(char c) {
        return (c >= 'a' && c <= 'z') || (c >= 'A' && c <= 'Z');
    }
}

public class GameSceneParser {
    public Dictionary<string, GameScene> scenes;
    List<Token> tokens;
    public GameSceneParser(string input) {
        this.tokens = GameSceneTranslater.Tokenize(input);
        this.scenes = new Dictionary<string, GameScene>();
        ParseScenes();
    }

    public void ParseScenes() {
        while (tokens.Count>0) {
            if (tokens[0] is SettingToken) {
                ParseScene();
            } else if (tokens[0] is EndToken || tokens[0] is IndentDecToken || tokens[0] is IndentIncToken) {
                tokens.RemoveAt(0);
            } else throw new CompileException("Expected new scene or end of file here.", tokens[0]);
        }
    }

    private void ParseScene() {
        string sceneName = null;
        Sprite sceneBackdrop = null;
        List<string> sceneCharacters = null;
        while (tokens.Count>0 && tokens[0] is SettingToken) {
            if (tokens.Count>=2 && tokens[1] is ColonToken) {
                string settingName = ((SettingToken)tokens[0]).data;
                tokens.RemoveRange(0,2);
                if (settingName == "scene") {
                    if (tokens.Count>0 && tokens[0] is IdentifierToken) {
                        sceneName = ((IdentifierToken)tokens[0]).data; 
                        tokens.RemoveAt(0);
                        EndCheck();
                    } else throw new CompileException("Expected scene name.", tokens[0]);
                } else if (settingName == "background") {
                    if (tokens.Count>0 && tokens[0] is IdentifierToken) {
                        var textureName = ((IdentifierToken)tokens[0]).data;
                        var texture = Resources.Load<Sprite>("Backgrounds/" + textureName);
                        if (texture == null) throw new CompileException("Background texture '" + textureName + "' not found.", tokens[0]);
                        sceneBackdrop = texture;
                        tokens.RemoveAt(0);
                        EndCheck();
                    } else throw new CompileException("Expected background name.", tokens[0]);
                } else if (settingName == "characters") {
                    if (tokens.Count>=1 && tokens[0] is IdentifierToken) {
                        sceneCharacters = new List<string>();
                        sceneCharacters.Add(((IdentifierToken)tokens[0]).data);
                        tokens.RemoveAt(0);
                        while (tokens.Count >= 1 && tokens[0] is CommaToken) {
                            if (tokens.Count >= 2 && tokens[1] is IdentifierToken) {
                                sceneCharacters.Add(((IdentifierToken)tokens[1]).data);
                                tokens.RemoveRange(0,2);
                            } else throw new CompileException("Expected another character name after comma.", tokens[0]);
                        }
                        EndCheck();
                    } else throw new CompileException("Expected at least one character name.", tokens[0]);
                }
            } else throw new CompileException("Expected colon after setting.", tokens[0]);
        }

        if (sceneName == null) throw new CompileException("Scene must have a '#scene' setting.");
        if (sceneBackdrop == null) throw new CompileException("Scene " + sceneName + " must have a '#background' setting");
        if (sceneCharacters == null) throw new CompileException("Scene " + sceneName + " must have a '#characters' setting");

        var tree = ParseStatements(false);

        scenes[sceneName] = new GameScene(sceneBackdrop, sceneCharacters.ToArray(), tree);
    }
    private DialogueTree ParseStatement() {
        if (tokens.Count >= 3 && tokens[0] is IdentifierToken && tokens[1] is ColonToken && tokens[2] is StringToken) {
            var msg = ((StringToken)tokens[2]).data;
            var name = ((IdentifierToken)tokens[0]).data;
            tokens.RemoveRange(0,3);
            EndCheck();
            return new DialogueMessage(msg, name, null);
        } else if (tokens.Count >= 1 && tokens[0] is ChoiceDefToken) {
            var choices = new Dictionary<string, (System.Func<bool>, DialogueTree)>();
            while (tokens.Count>0 && tokens[0] is ChoiceDefToken) {
                if (tokens.Count>=2 && tokens[0] is ChoiceDefToken && tokens[1] is StringToken) {
                    var msg = ((StringToken)tokens[1]).data;
                    tokens.RemoveRange(0,2);
                    EndCheck();
                    var code = ParseStatements(true);
                    choices[msg] = (null, code);
                } else if (tokens.Count>=3 && tokens[0] is ChoiceDefToken && tokens[1] is CodeToken && tokens[2] is StringToken) {
                    var condition = ParseCondition(((CodeToken)tokens[1]).data);
                    var msg = ((StringToken)tokens[2]).data;
                    tokens.RemoveRange(0,3);
                    EndCheck();
                    var code = ParseStatements(true);
                    choices[msg] = (condition, code);
                } else throw new CompileException("Expected a valid choice statement here.", tokens[0]);
            }
            return new DialogueChoice(choices);
        } else if (tokens.Count >= 1 && tokens[0] is CodeToken) {
            var code = ParseCodeStatement(((CodeToken)tokens[0]).data);
            tokens.RemoveAt(0);
            EndCheck();
            return new ArbitraryCodeNode(code, null);
        } else if (tokens.Count >= 2 && tokens[0] is ArrowToken && tokens[1] is IdentifierToken) {
            var name = ((IdentifierToken)tokens[1]).data;
            tokens.RemoveRange(0,2);
            EndCheck();
            if (name == "END") return new DebugEnd();
            else return new SceneChange(name);
        } else throw new CompileException("Expected a valid statement here.", tokens[0]);
    }

    private DialogueTree ParseStatements(bool expectIndent) {
        if (expectIndent)
            if (tokens.Count > 0 && tokens[0] is IndentIncToken) tokens.RemoveAt(0);
            else throw new CompileException("Expected a an indented statement block after choice.", tokens[0]);
            
        DialogueTree tree = null;
        while (tokens.Count > 0) {
            var tk = tokens[0];
            if (expectIndent) {
                if (tk is IndentDecToken) {
                    tokens.RemoveAt(0);
                    break;
                } 
            } else {
                if (tk is SettingToken || tk is EndToken || tk is IndentDecToken) break;
            }
            if (tk is IdentifierToken || tk is ChoiceDefToken || tk is CodeToken || tk is ArrowToken) {
                var next = ParseStatement();
                if (tree != null) tree.PushTree(next);
                else tree = next;
                continue;
            }
            throw new CompileException("Unrecognized token. Expected a statement, newline, or new scene.", tk);
        }
        return tree;
    }

    private System.Func<bool> ParseCondition(List<Token> code) {
        if (code.Count == 3 && code[0] is IdentifierToken && code[1] is IdentifierToken && (code[2] is IntToken || code[2] is IdentifierToken)) {
            var ident = ((IdentifierToken)code[0]).data;
            var type = ((IdentifierToken)code[1]).data;
            if (code[2] is IntToken) {
                var num = ((IntToken)code[2]).data;
                if      (type == ">")   return () => GameVariables.Get(ident) >  num;
                else if (type == ">=")  return () => GameVariables.Get(ident) >= num;
                else if (type == "<")   return () => GameVariables.Get(ident) <  num;
                else if (type == "<=")  return () => GameVariables.Get(ident) <= num;
                else if (type == "=")   return () => GameVariables.Get(ident) == num;
                else if (type == "!=")  return () => GameVariables.Get(ident) != num;
                else throw new CompileException("Invalid condition. Operator must be one of: >, <, >=, <=, =, !=.", code[1]);
            } else {
                var other = ((IdentifierToken)code[2]).data;
                if      (type == ">")   return () => GameVariables.Get(ident) >  GameVariables.Get(other);
                else if (type == ">=")  return () => GameVariables.Get(ident) >= GameVariables.Get(other);
                else if (type == "<")   return () => GameVariables.Get(ident) <  GameVariables.Get(other);
                else if (type == "<=")  return () => GameVariables.Get(ident) <= GameVariables.Get(other);
                else if (type == "=")   return () => GameVariables.Get(ident) == GameVariables.Get(other);
                else if (type == "!=")  return () => GameVariables.Get(ident) != GameVariables.Get(other);
                else throw new CompileException("Invalid condition. Operator must be one of: >, <, >=, <=, =, !=.", code[1]);
            }
        } else {
            throw new CompileException("Condition must be in the format: 'variable operator value'.", code[0]);
        }
    }
    private System.Action ParseCodeStatement(List<Token> code) {
        if (code.Count == 3 && code[0] is IdentifierToken && code[1] is IdentifierToken && (code[2] is IntToken || code[2] is IdentifierToken)) {
            var ident = ((IdentifierToken)code[0]).data;
            var type = ((IdentifierToken)code[1]).data;
            if (code[2] is IntToken) {
                var num = ((IntToken)code[2]).data;
                if      (type == "=")   return () => GameVariables.Set(ident, num);
                else if (type == "+")   return () => GameVariables.Set(ident, GameVariables.Get(ident) + num);
                else if (type == "-")   return () => GameVariables.Set(ident, GameVariables.Get(ident) - num);
                else if (type == "*")   return () => GameVariables.Set(ident, GameVariables.Get(ident) * num);
                else if (type == "/")   return () => GameVariables.Set(ident, GameVariables.Get(ident) / num);
                else throw new CompileException("Invalid expression. Operator must be one of: =, +, -, *, /.", code[1]);
            } else {
                var other = ((IdentifierToken)code[2]).data;
                if      (type == "=")   return () => GameVariables.Set(ident, GameVariables.Get(other));
                else if (type == "+")   return () => GameVariables.Set(ident, GameVariables.Get(ident) + GameVariables.Get(other));
                else if (type == "-")   return () => GameVariables.Set(ident, GameVariables.Get(ident) - GameVariables.Get(other));
                else if (type == "*")   return () => GameVariables.Set(ident, GameVariables.Get(ident) * GameVariables.Get(other));
                else if (type == "/")   return () => GameVariables.Set(ident, GameVariables.Get(ident) / GameVariables.Get(other));
                else throw new CompileException("Invalid expression. Operator must be one of: =, +, -, *, /.", code[1]);
            }
        } else {
            throw new CompileException("Condition must be in the format: 'variable operator value'.", code[0]);
        }
    }

    private void EndCheck() {
        if (tokens.Count > 0) {
            if (tokens[0] is EndToken) {
                tokens.RemoveAt(0);
            } else if (tokens.Count>1 && (tokens[0] is IndentDecToken || tokens[0] is IndentIncToken) && tokens[1] is EndToken) {
                tokens.RemoveRange(0,2);
            } else {
                throw new CompileException("Expected a newline here.", tokens[0]);
            }
        }
    }
}

public class CompileException : System.Exception {
    public CompileException(string msg) : base(msg) {}
    public CompileException(string msg, Token t) : base(msg + "\nAt " + t.lineNumber + ":" + t.lineIndex) {}
}
























/////////////////////////////////////////////////////////////////
// Besoke, evil hack to simulate rust enums. Tread no further. //
/////////////////////////////////////////////////////////////////

public class Token {
    public int lineNumber, lineIndex;
    public Token(int lineNum, int lineIndex) {
        this.lineNumber = lineNum;
        this.lineIndex = lineIndex;
    }
}
public class EmptyToken : Token {
    public EmptyToken(int lineNum, int lineIndex) : base (lineNum, lineIndex) {}
}
public class ColonToken : EmptyToken {
    public ColonToken(int lineNum, int lineIndex) : base (lineNum, lineIndex) {}
}
public class EndToken : EmptyToken {
    public EndToken(int lineNum, int lineIndex) : base (lineNum, lineIndex) {}
}
public class CommaToken : EmptyToken {
    public CommaToken(int lineNum, int lineIndex) : base (lineNum, lineIndex) {}
}
public class ChoiceDefToken : EmptyToken {
    public ChoiceDefToken(int lineNum, int lineIndex) : base (lineNum, lineIndex) {}
}
public class ArrowToken : EmptyToken {
    public ArrowToken(int lineNum, int lineIndex) : base (lineNum, lineIndex) {}
}
public class IndentIncToken : EmptyToken {
    public IndentIncToken(int lineNum, int lineIndex) : base (lineNum, lineIndex) {}
}
public class IndentDecToken : EmptyToken {
    public IndentDecToken(int lineNum, int lineIndex) : base (lineNum, lineIndex) {}
}

public class DataToken<T> : Token {
    public T data;
    public DataToken(T t, int lineNum, int lineIndex) : base(lineNum, lineIndex) {
        this.data = t;
    }
    public override string ToString()
    {
        return base.ToString() + "(" + this.data + ")";
    }
}
public class SettingToken : DataToken<string> {
    public SettingToken(string s, int lineNum, int lineIndex) : base(s,lineNum, lineIndex) {}
}
public class IdentifierToken : DataToken<string> {
    public IdentifierToken(string s, int lineNum, int lineIndex) : base(s,lineNum, lineIndex) {}
}
public class IntToken : DataToken<int> {
    public IntToken(int s, int lineNum, int lineIndex) : base(s,lineNum, lineIndex) {}
}
public class CodeToken : DataToken<List<Token>> {
    public CodeToken(List<Token> s, int lineNum, int lineIndex) : base(s,lineNum, lineIndex) {}
}
public class StringToken : DataToken<string> {
    public StringToken(string s, int lineNum, int lineIndex) : base(s,lineNum, lineIndex) {}
}