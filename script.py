import os
import re
from pathlib import Path

def collect_csharp_code(solution_path, output_file="combined_code.txt"):
    """
    Собирает весь код из C# проектов в решении в один файл
    """
    solution_path = Path(solution_path).resolve()
    output_file = Path(output_file)
    
    # Папки, которые нужно игнорировать
    ignore_dirs = {
        'bin', 'obj', '.git', '.vs', 'packages', 
        'node_modules', '__pycache__', 'Debug', 'Release',
        'x64', 'x86', 'TestResults', '.idea', '.vscode'
    }
    
    # Расширения файлов, которые нужно обработать
    code_extensions = {'.cs', '.csproj', '.sln', '.json', '.config', '.xml', '.proto', '.html'}
    
    # Файлы, которые нужно игнорировать
    ignore_files = {
        'packages.config', 'AssemblyInfo.cs', '*.Designer.cs',
        '*.generated.cs', 'TemporaryGeneratedFile_*.cs'
    }
    
    collected_files = []
    
    def should_include_file(file_path):
        """Определяем, нужно ли включать файл"""
        rel_path = file_path.relative_to(solution_path)
        
        # Проверяем, не в игнорируемой ли папке
        for part in rel_path.parts:
            if part in ignore_dirs:
                return False
        
        # Проверяем расширение
        if file_path.suffix.lower() not in code_extensions:
            return False
        
        # Проверяем имя файла
        for pattern in ignore_files:
            if pattern.endswith('*'):
                if file_path.name.endswith(pattern[1:]):
                    return False
            elif file_path.name == pattern:
                return False
        
        return True
    
    # Собираем все файлы
    for root, dirs, files in os.walk(solution_path):
        # Исключаем игнорируемые папки
        dirs[:] = [d for d in dirs if d not in ignore_dirs]
        
        for file in files:
            file_path = Path(root) / file
            if should_include_file(file_path):
                collected_files.append(file_path)
    
    # Сортируем файлы для сохранения структуры
    collected_files.sort(key=lambda x: str(x).lower())
    
    # Обрабатываем и записываем файлы
    with open(output_file, 'w', encoding='utf-8') as out_f:
        out_f.write(f"=== C# SOLUTION STRUCTURE ===\n")
        out_f.write(f"Solution Path: {solution_path}\n")
        out_f.write(f"Total Files: {len(collected_files)}\n\n")
        
        for file_path in collected_files:
            try:
                # Получаем относительный путь
                rel_path = file_path.relative_to(solution_path)
                
                # Читаем содержимое файла
                with open(file_path, 'r', encoding='utf-8', errors='ignore') as f:
                    content = f.read()
                
                # Минимизируем лишние пробелы для .cs файлов
                if file_path.suffix.lower() == '.cs':
                    # Удаляем множественные пустые строки
                    content = re.sub(r'\n\s*\n\s*\n', '\n\n', content)
                    # Удаляем лишние пробелы в начале строк
                    content = '\n'.join(line.rstrip() for line in content.split('\n'))
                
                # Записываем разделитель и информацию о файле
                out_f.write(f"\n{'='*60}\n")
                out_f.write(f"FILE: {rel_path}\n")
                out_f.write(f"SIZE: {len(content)} chars\n")
                out_f.write(f"{'='*60}\n\n")
                
                # Записываем содержимое
                out_f.write(content)
                out_f.write('\n')
                
                print(f"Processed: {rel_path}")
                
            except Exception as e:
                print(f"Error processing {file_path}: {e}")
    
    print(f"\n✓ Всего обработано файлов: {len(collected_files)}")
    print(f"✓ Результат сохранен в: {output_file}")
    print(f"✓ Размер выходного файла: {output_file.stat().st_size / 1024:.2f} KB")

def main():
    """Основная функция"""
    import sys
    
    if len(sys.argv) > 1:
        solution_path = sys.argv[1]
    else:
        # Ищем .sln файл в текущей директории
        sln_files = list(Path.cwd().glob("*.slnx"))
        if sln_files:
            solution_path = sln_files[0].parent
            print(f"Найдено решение: {sln_files[0]}")
        else:
            solution_path = input("Введите путь к решению (.sln файлу или папке): ").strip()
    
    if not Path(solution_path).exists():
        print("❌ Указанный путь не существует!")
        return
    
    output_file = "combined_code.txt"
    if len(sys.argv) > 2:
        output_file = sys.argv[2]
    
    collect_csharp_code(solution_path, output_file)

if __name__ == "__main__":
    main()