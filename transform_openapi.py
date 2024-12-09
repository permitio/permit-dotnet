import argparse
import json
from pathlib import Path


def transform_union_schema(input_path: Path, output_path: Path):
    """
    Transform an OpenAPI schema with union response types to the first type in the "anyOf" list.
    :param input_path: Path to the input JSON file.
    :param output_path: Path to the output JSON file.
    :return: None
    """
    with input_path.open("r", encoding="utf-8") as infile:
        data = json.load(infile)

    paths = data.get("paths", {})
    for path, path_data in paths.items():
        for method, method_data in path_data.items():
            responses = method_data.get("responses", {})
            for status_code, response_data in responses.items():
                content = response_data.get("content", {})
                for content_type, content_data in content.items():
                    schema = content_data.get("schema", {})
                    if "anyOf" in schema:
                        any_of = schema.pop("anyOf")
                        first_any_of: dict = next((
                            item for item in any_of if item.get("$ref", "").startswith("#/components/schemas/PaginatedResult_")
                        ), None)
                        if first_any_of is None:
                            continue

                        if "$ref" in first_any_of:
                            schema["$ref"] = first_any_of.get("$ref")
                        else:
                            items = first_any_of.get("items", {})
                            schema["$ref"] = items.get("$ref")
                            if "type" in items:
                                schema["type"] = items.get("type")

                        print(f"Transforming schema for {path!r} {method=} {status_code=} {content_type=}, taking first type in 'anyOf' list: {schema['$ref']}")

    # Save the transformed JSON to the output file
    with output_path.open("w", encoding="utf-8") as outfile:
        json.dump(data, outfile, indent=2)

    print(f"Transformed JSON saved to {output_path}")


def main():
    # Set up command-line argument parsing
    parser = argparse.ArgumentParser(description="Transform JSON schema by simplifying 'anyOf' structure.")
    parser.add_argument("input_file", type=Path, help="Path to the input JSON file.")
    parser.add_argument("output_file", type=Path, help="Path to the output JSON file.")
    args = parser.parse_args()
    # Perform the schema transformation
    transform_union_schema(args.input_file, args.output_file)


if __name__ == "__main__":
    main()
