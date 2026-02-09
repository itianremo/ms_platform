import 'package:flutter/material.dart';

class FilterModal extends StatefulWidget {
  final Function(int? minAge, int? maxAge, String? country) onApply;

  const FilterModal({required this.onApply});

  @override
  State<FilterModal> createState() => _FilterModalState();
}

class _FilterModalState extends State<FilterModal> {
  RangeValues _ageRange = const RangeValues(18, 50);
  String? _selectedCountry;

  @override
  Widget build(BuildContext context) {
    return Container(
      padding: const EdgeInsets.all(24),
      child: Column(
        mainAxisSize: MainAxisSize.min,
        crossAxisAlignment: CrossAxisAlignment.start,
        children: [
          const Text('Filters',
              style: TextStyle(fontSize: 24, fontWeight: FontWeight.bold)),
          const SizedBox(height: 24),
          const Text('Age Range'),
          RangeSlider(
            values: _ageRange,
            min: 18,
            max: 100,
            divisions: 82,
            labels: RangeLabels(
              _ageRange.start.round().toString(),
              _ageRange.end.round().toString(),
            ),
            onChanged: (values) {
              setState(() {
                _ageRange = values;
              });
            },
          ),
          const SizedBox(height: 24),
          const Text('Country'),
          DropdownButtonFormField<String>(
            value: _selectedCountry,
            items: ['Egypt', 'Germany', 'USA', 'France']
                .map((c) => DropdownMenuItem(value: c, child: Text(c)))
                .toList(),
            onChanged: (v) => setState(() => _selectedCountry = v),
          ),
          const SizedBox(height: 32),
          SizedBox(
            width: double.infinity,
            child: ElevatedButton(
              onPressed: () {
                widget.onApply(_ageRange.start.round(), _ageRange.end.round(),
                    _selectedCountry);
                Navigator.pop(context);
              },
              child: const Text('Apply'),
            ),
          ),
        ],
      ),
    );
  }
}
