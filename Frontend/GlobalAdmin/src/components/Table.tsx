import React from 'react';

interface Column<T> {
    header: string;
    accessor: keyof T | ((item: T) => React.ReactNode);
    width?: string;
}

interface TableProps<T> {
    data: T[];
    columns: Column<T>[];
    onRowClick?: (item: T) => void;
}

export function Table<T>({ data, columns, onRowClick }: TableProps<T>) {
    return (
        <div style={{ overflowX: 'auto', borderRadius: 'var(--radius)', border: '1px solid var(--border)' }}>
            <table style={{ width: '100%', borderCollapse: 'collapse', textAlign: 'left', fontSize: '0.875rem' }}>
                <thead style={{ backgroundColor: 'var(--bg-card)', borderBottom: '1px solid var(--border)' }}>
                    <tr>
                        {columns.map((col, idx) => (
                            <th key={idx} style={{ padding: '0.75rem 1rem', fontWeight: '600', color: 'var(--text-secondary)' }}>
                                {col.header}
                            </th>
                        ))}
                    </tr>
                </thead>
                <tbody>
                    {data.map((item, rowIdx) => (
                        <tr
                            key={rowIdx}
                            onClick={() => onRowClick && onRowClick(item)}
                            style={{
                                borderBottom: rowIdx < data.length - 1 ? '1px solid var(--border)' : 'none',
                                cursor: onRowClick ? 'pointer' : 'default',
                                backgroundColor: 'rgba(255, 255, 255, 0.02)'
                            }}
                        >
                            {columns.map((col, colIdx) => (
                                <td key={colIdx} style={{ padding: '0.75rem 1rem' }}>
                                    {typeof col.accessor === 'function'
                                        ? col.accessor(item)
                                        : (item[col.accessor] as React.ReactNode)}
                                </td>
                            ))}
                        </tr>
                    ))}
                </tbody>
            </table>
        </div>
    );
}
